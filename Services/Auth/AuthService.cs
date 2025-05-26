using BlogBackend.Data;
using BlogBackend.Dtos.Auth;
using BlogBackend.Helpers;
using BlogBackend.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System;
using System.Threading.Tasks;
using BlogBackend.Services; // Added this to correctly reference EmailService
using Microsoft.Extensions.Configuration;

namespace BlogBackend.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly BlogDbContext _context;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(BlogDbContext context, JwtTokenHelper jwtTokenHelper, EmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _jwtTokenHelper = jwtTokenHelper;
            _emailService = emailService;
            _configuration = configuration;
        }

        /// <summary>
        /// Register a new user and send email confirmation.
        /// </summary>
        public async Task<object> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(x => x.Email == registerDto.Email))
                throw new Exception("Email already exists.");

            // Use a transaction to ensure atomicity
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Create a new user
                    bool isFirstUser = !await _context.Users.AnyAsync();
                    var user = new User
                    {
                        Username = registerDto.Username,
                        Email = registerDto.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                        EmailConfirmationToken = TokenGenerator.GenerateSecureToken(),
                        EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
                        Role = isFirstUser ? "Admin" : "User"
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Professional HTML confirmation email with backend link for redirect
                    var backendUrl = "http://localhost:5246/api/Auth/confirm-email?token=" + user.EmailConfirmationToken;
                    var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirm Your Email</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <h2 style='color: #2c3e50; margin-bottom: 20px;'>Welcome to CybersecBlog!</h2>
        <p>Thank you for registering with CybersecBlog. We're excited to have you join our community!</p>
        <p>To complete your registration and activate your account, please click the button below:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{backendUrl}'
               style='background-color: #3498db; color: #fff; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>
                Confirm Email Address
            </a>
        </div>
        <p>If the button doesn't work, you can also copy and paste the following link into your browser:</p>
        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; word-break: break-all;'>
            <a href='{backendUrl}' style='color: #3498db; text-decoration: none;'>{backendUrl}</a>
        </div>
        <p style='color: #e74c3c; font-weight: bold;'>This link will expire in 24 hours for security reasons.</p>
        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
            <p>Best regards,<br>The CybersecBlog Team</p>
        </div>
        <div style='margin-top: 20px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666;'>
            <p>This is an automated message. Please do not reply to this email.<br>
            If you need assistance, please contact our support team.</p>
        </div>
    </div>
</body>
</html>";

                    await _emailService.SendEmailAsync(user.Email, "Welcome to CybersecBlog - Confirm Your Email", emailBody);

                    await transaction.CommitAsync();

                    // Return a success message or user details without the token
                    return new
                    {
                        Username = user.Username,
                        Email = user.Email,
                        Message = "Registration successful. Please confirm your email."
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Authenticate user with email and password.
        /// </summary>
        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);

            // Validate credentials
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password.");

            // Check if email is confirmed
            if (!user.EmailConfirmed)
                throw new Exception("Email not confirmed.");

            // Generate and return JWT token
            return _jwtTokenHelper.GenerateToken(user.Id, user.Username, user.Role);
        }

        /// <summary>
        /// Confirm user's email address using a confirmation token.
        /// </summary>
        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.EmailConfirmationToken != token)
                return false;

            // Confirm email and clear the token
            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Handle forgot password request and send password reset token.
        /// </summary>
        public async Task<(bool success, string token)> ForgotPasswordAsync(string email)
        {
            Console.WriteLine($"Debug - ForgotPasswordAsync started for email: {email}");
            
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine($"Debug - No user found with email: {email}");
                return (false, null);
            }
            Console.WriteLine($"Debug - User found: {user.Username}");

            // Generate password reset token and save
            user.PasswordResetToken = TokenGenerator.GenerateSecureToken();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours
            Console.WriteLine($"Debug - Generated reset token: {user.PasswordResetToken}");

            try 
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("Debug - Token saved to database successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug - Error saving token to database: {ex.Message}");
                throw;
            }

            // Get the frontend URL from configuration
            var frontendUrl = _configuration["AppSettings:FrontendUrl"];
            Console.WriteLine($"Debug - Frontend URL from config: {frontendUrl}");
            frontendUrl = frontendUrl ?? "http://localhost:5086";
            Console.WriteLine($"Debug - Final Frontend URL: {frontendUrl}");
            
            // Create the reset password link without the Auth prefix
            var resetLink = $"{frontendUrl}/reset-password?token={user.PasswordResetToken}&email={Uri.EscapeDataString(email)}";
            Console.WriteLine("Debug - Generated reset link: " + resetLink);

            // Create a professional email body with inline styles
            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <h2 style='color: #2c3e50; margin-bottom: 20px;'>Password Reset Request</h2>
        
        <p>Dear {user.Username},</p>
        
        <p>We received a request to reset your password for your CybersecBlog account. If you didn't make this request, you can safely ignore this email.</p>
        
        <p>To reset your password, click the button below:</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}' 
               style='background-color: #3498db; 
                      color: #ffffff; 
                      padding: 12px 30px; 
                      text-decoration: none; 
                      border-radius: 5px; 
                      font-weight: bold;
                      display: inline-block;'>
                Reset Password
            </a>
        </div>

        <p>If the button above doesn't work, copy and paste this link into your browser:</p>
        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; word-break: break-all;'>
            <a href='{resetLink}' style='color: #3498db; text-decoration: none;'>{resetLink}</a>
        </div>

        <p style='color: #e74c3c; font-weight: bold;'>Important: This password reset link will expire in 24 hours for security reasons.</p>
        
        <p>If you didn't request a password reset, please contact our support team immediately.</p>
        
        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
            <p>Best regards,<br>The CybersecBlog Team</p>
        </div>
        
        <div style='margin-top: 20px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666;'>
            <p>This is an automated message. Please do not reply to this email.<br>
            If you need assistance, please contact our support team.</p>
        </div>
    </div>
</body>
</html>";

            Console.WriteLine("Debug - Email body generated");

            // Send password reset email
            try
            {
                Console.WriteLine($"Debug - Attempting to send email to {user.Email}");
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Reset Your CybersecBlog Password",
                    emailBody
                );
                Console.WriteLine($"Debug - Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug - Error sending email: {ex.Message}");
                Console.WriteLine($"Debug - Stack trace: {ex.StackTrace}");
                throw;
            }

            // Return both success status and token
            return (true, user.PasswordResetToken);
        }

        /// <summary>
        /// Reset user's password using the provided reset token.
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.PasswordResetToken != token)
                return false;

            // Update the user's password and clear the reset token
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}