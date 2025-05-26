using BlogBackend.Dtos.Comment;

namespace BlogBackend.Services.Interfaces
{
    public interface ICommentService
    {
        Task<int> CreateCommentAsync(int postId, int userId, CreateCommentDto createCommentDto);
        Task<IEnumerable<CommentViewDto>> GetCommentsByPostIdAsync(int postId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);

    }
}
