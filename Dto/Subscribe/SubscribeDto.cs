using System.ComponentModel.DataAnnotations;

namespace BlogBackend.Dtos.Subscribe
{
    public class SubscribeDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        public string? Name { get; set; }
    }

    public class UnsubscribeDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UnsubscribeToken { get; set; }
    }

    public class SubscriptionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? UnsubscribeToken { get; set; }
    }
} 