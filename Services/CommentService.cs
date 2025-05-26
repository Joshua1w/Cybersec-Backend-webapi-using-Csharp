using BlogBackend.Data;
using BlogBackend.Dtos.Comment;
using BlogBackend.Models;
using BlogBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace BlogBackend.Services
{
    public class CommentService : ICommentService
    {
        private readonly BlogDbContext _context;
        private readonly IMapper _mapper;

        public CommentService(BlogDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> CreateCommentAsync(int postId, int userId, CreateCommentDto createCommentDto)
        {
            var comment = _mapper.Map<Comment>(createCommentDto);
            comment.UserId = userId;
            comment.PostId = postId;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment.Id;
        }

        public async Task<IEnumerable<CommentViewDto>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommentViewDto>>(comments);
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return false;

            // Allow if owner or admin
            if (comment.UserId != userId && user.Role != "Admin")
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
