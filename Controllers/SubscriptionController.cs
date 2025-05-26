using Microsoft.AspNetCore.Mvc;
using BlogBackend.Services.Interfaces;
using BlogBackend.Dtos.Subscribe;
using Microsoft.AspNetCore.Authorization;

namespace BlogBackend.Controllers
{
    [ApiController]
    [Route("api/subscription")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost("subscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto subscribeDto)
        {
            try
            {
                var result = await _subscriptionService.SubscribeAsync(subscribeDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("unsubscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> UnsubscribeGet([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                var unsubscribeDto = new UnsubscribeDto
                {
                    Email = email,
                    UnsubscribeToken = token
                };
                var result = await _subscriptionService.UnsubscribeAsync(unsubscribeDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("unsubscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> UnsubscribePost([FromBody] UnsubscribeDto unsubscribeDto)
        {
            try
            {
                var result = await _subscriptionService.UnsubscribeAsync(unsubscribeDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("check")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckSubscription([FromQuery] string email)
        {
            try
            {
                var isSubscribed = await _subscriptionService.IsSubscribedAsync(email);
                return Ok(new { IsSubscribed = isSubscribed });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
} 