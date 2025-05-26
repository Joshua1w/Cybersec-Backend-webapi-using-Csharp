namespace BlogBackend.Services.Interfaces
{
    public interface ILikeService
    {
        Task<bool> LikePostAsync(int postId, int userId);
        Task<bool> UnlikePostAsync(int postId, int userId);
        Task<int> GetLikeCountAsync(int postId);
        Task<(bool liked, int count)> ToggleLikeAsync(int postId, int userId);
    }
}
