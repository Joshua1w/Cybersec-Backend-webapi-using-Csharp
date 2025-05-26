namespace BlogBackend.Dtos.Comment
{
    public class CommentViewDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
