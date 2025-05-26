using System.ComponentModel.DataAnnotations;

namespace BlogBackend.Models
{
    public class User
    {
        public User()
        {
            Posts = new List<Post>();
            Comments = new List<Comment>();
            Likes = new List<PostLike>();
            Role = "User";
        }

        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; }

        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostLike> Likes { get; set; }
    }
}
