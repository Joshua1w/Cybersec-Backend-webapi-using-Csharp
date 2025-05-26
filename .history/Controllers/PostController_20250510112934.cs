using BlogBackend.Dtos.Post;
using BlogBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogBackend.Controllers
{
    [ApiController]
    [Route("api/post")]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            if (createPostDto == null)
            {
                return BadRequest(new { Message = "Invalid post data." });
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var postId = await _postService.CreatePostAsync(userId, createPostDto);
                return Ok(new { PostId = postId });
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your logging mechanism)
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred while creating the post." });
            }
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _postService.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving posts." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            try
            {
                Console.WriteLine($"JWT present: {Request.Headers.ContainsKey("Authorization")}");
                Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
                int? userId = null;
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    Console.WriteLine($"UserId claim: {claim}");
                    if (int.TryParse(claim, out int uid))
                        userId = uid;
                }
                var post = await _postService.GetPostByIdAsync(id, userId);
                Console.WriteLine($"Returned post.IsLiked: {post?.IsLiked}");
                if (post == null)
                {
                    return NotFound(new { Message = "Post not found." });
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving the post." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto updatePostDto)
        {
            if (updatePostDto == null)
            {
                return BadRequest(new { Message = "Invalid post data." });
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                Console.WriteLine($"Edit attempt by user {userId} for post {id}");
                var updated = await _postService.UpdatePostAsync(id, userId, updatePostDto);
                if (!updated)
                {
                    return Forbid(new { Message = "You are not authorized to update this post." });
                }

                return Ok(new { Message = "Post updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred while updating the post." });
            }
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var deleted = await _postService.DeletePostAsync(id, userId);
                if (!deleted)
                {
                    return Forbid(new { Message = "You are not authorized to delete this post." });
                }

                return Ok(new { Message = "Post deleted successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred while deleting the post." });
            }
        }

        private IActionResult Forbid(object value)
        {
            return StatusCode(403, value);
        }

        [HttpGet("my-posts")]
        public async Task<IActionResult> GetMyPosts()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var posts = await _postService.GetPostsByUserAsync(userId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred while retrieving your posts." });
            }
        }
    }
}