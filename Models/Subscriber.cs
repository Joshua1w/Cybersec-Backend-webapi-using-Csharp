using System.ComponentModel.DataAnnotations;

namespace BlogBackend.Models
{
    public class Subscriber
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Optional fields for better subscriber management
        public string? Name { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
        public string? UnsubscribeToken { get; set; }
    }
} 