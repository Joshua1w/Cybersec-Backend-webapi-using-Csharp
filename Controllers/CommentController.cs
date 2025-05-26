using BlogBackend.Dtos.Comment;
using BlogBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogBackend.Controllers
{
    [ApiController]
    [Route("api/comment")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("{postId}")]
        public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentDto createCommentDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var commentId = await _commentService.CreateCommentAsync(postId, userId, createCommentDto);
            return Ok(new { CommentId = commentId });
        }

        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentsByPostId(int postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var deleted = await _commentService.DeleteCommentAsync(commentId, userId);
            if (!deleted) return Forbid();

            return Ok("Comment deleted successfully.");
        }

    }
}
