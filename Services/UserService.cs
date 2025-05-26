using BlogBackend.Dtos.Auth;
using BlogBackend.Models;
using BlogBackend.Services.Interfaces;
using BlogBackend.Data;
using BlogBackend.Helpers; // Added for JwtTokenHelper
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;

namespace BlogBackend.Services
{
    public class UserService : IUserService
    {
        private readonly BlogDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtTokenHelper _jwtTokenHelper; // Added JwtTokenHelper
        private readonly EmailService _emailService;

        public UserService(BlogDbContext context, IMapper mapper, JwtTokenHelper jwtTokenHelper, EmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _jwtTokenHelper = jwtTokenHelper; // Initialize JwtTokenHelper
            _emailService = emailService; // Initialize EmailService
        }

        // Removed duplicate RegisterAsync method. Use AuthService.RegisterAsync instead.

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            // Find user by plain token
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.EmailConfirmationToken == token && 
                u.EmailConfirmationTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return false;

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            if (!user.EmailConfirmed)
            {
                return new AuthResponseDto
                {
                    Username = user.Username,
                    Email = user.Email,
                    Message = "Please confirm your email before logging in.",
                    Token = null
                };
            }

            // Generate JWT token only for authenticated and confirmed users
            var token = _jwtTokenHelper.GenerateToken(user.Id, user.Username, user.Role);

            return new AuthResponseDto
            {
                Username = user.Username,
                Email = user.Email,
                Token = token,
                Message = "Login successful."
            };
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            user.PasswordResetToken = TokenGenerator.GenerateSecureToken();
            await _context.SaveChangesAsync();
            return user.PasswordResetToken;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.PasswordResetToken == token);
            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }

        // Fetch all users (admin use)
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            // Prevent deleting the first ever admin
            var firstAdmin = await _context.Users.Where(u => u.Role == "Admin").OrderBy(u => u.Id).FirstOrDefaultAsync();
            if (firstAdmin != null && user.Id == firstAdmin.Id)
                throw new InvalidOperationException("Cannot delete the first ever admin user.");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Service-layer protection for demoting first admin
        public async Task<bool> DemoteAdminAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.Role != "Admin") return false;
            var firstAdmin = await _context.Users.Where(u => u.Role == "Admin").OrderBy(u => u.Id).FirstOrDefaultAsync();
            if (firstAdmin != null && user.Id == firstAdmin.Id)
                throw new InvalidOperationException("Cannot demote the first ever admin user.");
            user.Role = "User";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}