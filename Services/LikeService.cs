using BlogBackend.Data;
using BlogBackend.Models;
using BlogBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogBackend.Services
{
    public class LikeService : ILikeService
    {
        private readonly BlogDbContext _context;

        public LikeService(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<(bool liked, int count)> ToggleLikeAsync(int postId, int userId)
        {
            var existingLike = await _context.PostLikes.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);
            if (existingLike != null)
            {
                _context.PostLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                var count = await _context.PostLikes.CountAsync(x => x.PostId == postId);
                return (false, count);
            }
            else
            {
                var like = new PostLike { PostId = postId, UserId = userId };
                _context.PostLikes.Add(like);
                await _context.SaveChangesAsync();
                var count = await _context.PostLikes.CountAsync(x => x.PostId == postId);
                return (true, count);
            }
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _context.PostLikes.CountAsync(x => x.PostId == postId);
        }

        public async Task<bool> LikePostAsync(int postId, int userId)
        {
            if (await _context.PostLikes.AnyAsync(x => x.PostId == postId && x.UserId == userId))
                return false;

            var like = new PostLike
            {
                PostId = postId,
                UserId = userId
            };

            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikePostAsync(int postId, int userId)
        {
            var like = await _context.PostLikes.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);
            if (like == null) return false;

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
