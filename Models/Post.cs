using System.ComponentModel.DataAnnotations;

namespace BlogBackend.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostLike> Likes { get; set; }

        // Many-to-many relationship with Category
        public ICollection<Category> Categories { get; set; }
    }
}
