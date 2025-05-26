using BlogBackend.Dtos.Post;

namespace BlogBackend.Services.Interfaces
{
    public interface IPostService
    {
        Task<int> CreatePostAsync(int userId, CreatePostDto createPostDto);
        Task<IEnumerable<PostViewDto>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10, int? categoryId = null);
        Task<PostViewDto> GetPostByIdAsync(int id, int? userId = null);
        Task<bool> UpdatePostAsync(int id, int userId, UpdatePostDto updatePostDto);
        Task<bool> DeletePostAsync(int id, int userId);
        Task<IEnumerable<PostViewDto>> GetPostsByUserAsync(int userId);
    }
}
