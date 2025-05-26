using BlogBackend.Dtos.Auth;

namespace BlogBackend.Services.Auth
{
    public interface IAuthService
    {
        Task<object> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<(bool success, string token)> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
