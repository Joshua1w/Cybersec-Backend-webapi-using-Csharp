using BlogBackend.Data;
using BlogBackend.Dtos.Post;
using BlogBackend.Models;
using BlogBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;


namespace BlogBackend.Services
{
    public class PostService : IPostService
    {
        private readonly BlogDbContext _context;
        private readonly IMapper _mapper;

        public PostService(BlogDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> CreatePostAsync(int userId, CreatePostDto createPostDto)
        {
            var post = _mapper.Map<Post>(createPostDto);
            post.UserId = userId;

            // Fetch and assign categories
            if (createPostDto.CategoryIds != null && createPostDto.CategoryIds.Any())
            {
                post.Categories = await _context.Categories
                    .Where(c => createPostDto.CategoryIds.Contains(c.Id))
                    .ToListAsync();
            }
            else
            {
                post.Categories = new List<Category>();
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post.Id;
        }

        public async Task<IEnumerable<PostViewDto>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10, int? categoryId = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.Categories)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));
            }

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PostViewDto>>(posts);
        }

        public async Task<PostViewDto> GetPostByIdAsync(int id, int? userId = null)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            var dto = _mapper.Map<PostViewDto>(post);
            if (dto != null && userId.HasValue && post != null)
            {
                dto.IsLiked = post.Likes.Any(like => like.UserId == userId.Value);
            }
            else if (dto != null)
            {
                dto.IsLiked = false;
            }
            return dto;
        }

        public async Task<bool> UpdatePostAsync(int id, int userId, UpdatePostDto updatePostDto)
        {
            var post = await _context.Posts
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (post == null) return false;

            post.Title = updatePostDto.Title;
            post.Content = updatePostDto.Content;

            // Update categories
            if (updatePostDto.CategoryIds != null)
            {
                post.Categories = await _context.Categories
                    .Where(c => updatePostDto.CategoryIds.Contains(c.Id))
                    .ToListAsync();
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostAsync(int id, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return false;

            // Allow if owner or admin
            if (post.UserId != userId && user.Role != "Admin")
                return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PostViewDto>> GetPostsByUserAsync(int userId)
        {
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PostViewDto>>(posts);
        }
    }
}
