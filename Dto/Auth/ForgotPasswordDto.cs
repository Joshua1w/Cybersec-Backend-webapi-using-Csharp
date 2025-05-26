using System.ComponentModel.DataAnnotations;

namespace BlogBackend.Dtos.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
} 