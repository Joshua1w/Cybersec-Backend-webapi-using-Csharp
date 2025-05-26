using BlogBackend.Dtos.Auth;
using BlogBackend.Models;

namespace BlogBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<User> GetUserByIdAsync(int id);
        Task<bool> ConfirmEmailAsync(string token);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<User> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<bool> DeleteUserAsync(int id);
        Task<bool> DemoteAdminAsync(string email);
    }
}
