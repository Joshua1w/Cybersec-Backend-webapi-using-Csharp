using BlogBackend.Dtos.Subscribe;

namespace BlogBackend.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDto> SubscribeAsync(SubscribeDto subscribeDto);
        Task<SubscriptionResponseDto> UnsubscribeAsync(UnsubscribeDto unsubscribeDto);
        Task<bool> IsSubscribedAsync(string email);
        Task<IEnumerable<string>> GetAllActiveSubscribersAsync();
    }
} 