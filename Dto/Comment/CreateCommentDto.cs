using System.ComponentModel.DataAnnotations;

namespace BlogBackend.Dtos.Comment
{
    public class CreateCommentDto
    {
        [Required]
        public string Content { get; set; }
    }
}
