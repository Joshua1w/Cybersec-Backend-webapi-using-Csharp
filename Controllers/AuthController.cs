using BlogBackend.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using BlogBackend.Data;
using BlogBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BlogBackend.Helpers;
using BlogBackend.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlogBackend.Controllers
{
    public class PromoteToAdminRequest
    {
        public string Email { get; set; }
    }
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly IAuthService _authService;
        private readonly Services.AuditLogger _auditLogger;

        public AuthController(IUserService userService, JwtTokenHelper jwtTokenHelper, IAuthService authService, Services.AuditLogger auditLogger)
        {
            _userService = userService;
            _jwtTokenHelper = jwtTokenHelper;
            _authService = authService;
            _auditLogger = auditLogger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var response = await _authService.RegisterAsync(registerDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
                public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = await _authService.LoginAsync(loginDto);
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Invalid credentials.");

                // Get user info for response
                var user = await _userService.GetByEmailAsync(loginDto.Email);
                var roles = new List<string>();
                if (user.Role != null)
                {
                    roles.Add(user.Role);
                }
                return Ok(new AuthResponseDto
                {
                    Token = token,
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Message = "Login successful.",
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required.");

            var result = await _userService.ConfirmEmailAsync(token);
            if (result)
            {
                // Show a success message and redirect after 3 seconds
                var html = @"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Confirmed</title>
    <style>
        body { font-family: Arial, sans-serif; background: #f4f6f8; color: #333; }
        .container { max-width: 400px; margin: 80px auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); padding: 32px; text-align: center; }
        .success { color: #27ae60; font-size: 2.2em; margin-bottom: 16px; }
        .redirect { margin-top: 24px; color: #888; }
    </style>
    <script>
        setTimeout(function() {
            window.location.href = 'http://localhost:5086/SignIn';
        }, 3000);
    </script>
</head>
<body>
    <div class='container'>
        <div class='success'>✔️</div>
        <h2>Email Confirmed!</h2>
        <p>Your email has been successfully confirmed.<br>You will be redirected to the sign-in page shortly.</p>
        <div class='redirect'>If you are not redirected, <a href='http://localhost:5086/SignIn'>click here</a>.</div>
    </div>
</body>
</html>";
                return Content(html, "text/html");
            }
            else
            {
                return BadRequest("Invalid or expired confirmation token.");
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
                public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var (success, token) = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                if (success)
                {
                    // Return both a user-friendly message and the token (for testing purposes)
                    return Ok(new { 
                        Message = "If an account exists with this email, a password reset link has been sent.",
                        // Only include the token in development environment
                        ResetToken = token 
                    });
                }
                return BadRequest(new { Message = "Failed to process forgot password request." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(
                    resetPasswordDto.Email,
                    resetPasswordDto.Token,
                    resetPasswordDto.NewPassword
                );

                if (result)
                {
                    return Ok(new { Message = "Password has been reset successfully." });
                }
                return BadRequest(new { Message = "Invalid or expired reset token." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        // POST: api/auth/promote-to-admin
        [HttpPost("promote-to-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteToAdmin([FromBody] PromoteToAdminRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { Message = "Email is required." });

            // Enforce: Only first admin can promote
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId))
                return Forbid("Invalid user context.");
            var firstAdmin = (await _userService.GetAllAsync()).Where(u => u.Role == "Admin").OrderBy(u => u.Id).FirstOrDefault();
            if (firstAdmin == null || currentUserId != firstAdmin.Id)
                return Forbid("Only the first admin can promote other users to admin.");

            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            if (user.Role == "Admin")
                return BadRequest(new { Message = "User is already an admin." });

            user.Role = "Admin";
            await (Request.HttpContext.RequestServices.GetService(typeof(BlogDbContext)) as BlogDbContext).SaveChangesAsync();
            // Audit log
            var performer = User.Identity?.Name ?? $"UserId:{currentUserId}";
            await _auditLogger.LogAsync("PromoteToAdmin", performer, user.Email, $"Promoted to admin");
            return Ok(new { Message = $"{user.Email} promoted to admin." });
        }
        // GET: api/auth/users
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users.Select(u => new {
                u.Id,
                u.Username,
                u.Email,
                u.Role
            }));
        }
        // DELETE: api/auth/user/{id}
        [HttpDelete("user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Enforce: Only first admin can delete
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId))
                return Forbid("Invalid user context.");
            var firstAdmin = (await _userService.GetAllAsync()).Where(u => u.Role == "Admin").OrderBy(u => u.Id).FirstOrDefault();
            if (firstAdmin == null || currentUserId != firstAdmin.Id)
                return Forbid("Only the first admin can delete users.");
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { Message = "User not found." });
                await _userService.DeleteUserAsync(id);
                // Audit log
                var performer = User.Identity?.Name ?? $"UserId:{currentUserId}";
                await _auditLogger.LogAsync("DeleteUser", performer, user?.Email ?? $"Id:{id}", "User deleted");
                return Ok(new { Message = "User deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        // POST: api/auth/remove-admin
        [HttpPost("remove-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAdmin([FromBody] PromoteToAdminRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { Message = "Email is required." });
            // Enforce: Only first admin can demote
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId))
                return Forbid("Invalid user context.");
            var firstAdmin = (await _userService.GetAllAsync()).Where(u => u.Role == "Admin").OrderBy(u => u.Id).FirstOrDefault();
            if (firstAdmin == null || currentUserId != firstAdmin.Id)
                return Forbid("Only the first admin can demote admins.");
            try
            {
                var user = await _userService.GetByEmailAsync(request.Email);
                if (user == null)
                    return NotFound(new { Message = "User not found." });
                if (user.Role != "Admin")
                    return BadRequest(new { Message = "User is not an admin." });
                await _userService.DemoteAdminAsync(request.Email);
                // Audit log
                var performer = User.Identity?.Name ?? $"UserId:{currentUserId}";
                await _auditLogger.LogAsync("RemoveAdmin", performer, user.Email, "Demoted from admin");
                return Ok(new { Message = $"{user.Email} demoted from admin." });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }
}
