namespace BlogBackend.Dtos.Post
{
    public class PostViewDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLiked { get; set; }
        public List<BlogBackend.Dtos.CategoryResponse> Categories { get; set; }
    }
}
