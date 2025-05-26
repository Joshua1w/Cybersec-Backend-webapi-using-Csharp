using BlogBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogBackend.Controllers
{
    [ApiController]
    [Route("api/like")]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpGet("{postId}/count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLikeCount(int postId)
        {
            var count = await _likeService.GetLikeCountAsync(postId);
            return Ok(new { count });
        }

        [HttpPost("{postId}/toggle")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var (liked, count) = await _likeService.ToggleLikeAsync(postId, userId);
            return Ok(new { liked, count });
        }

        [HttpPost("{postId}")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var liked = await _likeService.LikePostAsync(postId, userId);
            var likeCount = await _likeService.GetLikeCountAsync(postId);
            if (!liked) return BadRequest(new { message = "Already liked.", likeCount });

            return Ok(new { message = "Post liked successfully.", likeCount });
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var unliked = await _likeService.UnlikePostAsync(postId, userId);
            var likeCount = await _likeService.GetLikeCountAsync(postId);
            if (!unliked) return BadRequest(new { message = "You haven't liked this post yet.", likeCount });

            return Ok(new { message = "Post unliked successfully.", likeCount });
        }
    }
}
