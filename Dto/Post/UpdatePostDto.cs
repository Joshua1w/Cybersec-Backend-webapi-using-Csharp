using System.ComponentModel.DataAnnotations;

namespace BlogBackend.Dtos.Post
{
    public class UpdatePostDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        // New: List of category IDs for the post
        public List<int> CategoryIds { get; set; }
    }
}
