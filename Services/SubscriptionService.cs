using BlogBackend.Data;
using BlogBackend.Dtos.Subscribe;
using BlogBackend.Models;
using BlogBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BlogBackend.Helpers;

namespace BlogBackend.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly BlogDbContext _context;
        private readonly EmailService _emailService;

        public SubscriptionService(BlogDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<SubscriptionResponseDto> SubscribeAsync(SubscribeDto subscribeDto)
        {
            var existingSubscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == subscribeDto.Email);

            if (existingSubscriber != null)
            {
                if (existingSubscriber.IsActive)
                {
                    return new SubscriptionResponseDto
                    {
                        Success = false,
                        Message = "This email is already subscribed to our newsletter."
                    };
                }

                // Reactivate subscription
                existingSubscriber.IsActive = true;
                existingSubscriber.UnsubscribedAt = null;
                await _context.SaveChangesAsync();

                return new SubscriptionResponseDto
                {
                    Success = true,
                    Message = "Your subscription has been reactivated.",
                    UnsubscribeToken = existingSubscriber.UnsubscribeToken
                };
            }

            var unsubscribeToken = TokenGenerator.GenerateSecureToken();
            var subscriber = new Subscriber
            {
                Email = subscribeDto.Email,
                Name = subscribeDto.Name,
                UnsubscribeToken = unsubscribeToken
            };

            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            // Send welcome email
            await SendWelcomeEmailAsync(subscriber);

            return new SubscriptionResponseDto
            {
                Success = true,
                Message = "You have successfully subscribed to our newsletter.",
                UnsubscribeToken = unsubscribeToken
            };
        }

        public async Task<SubscriptionResponseDto> UnsubscribeAsync(UnsubscribeDto unsubscribeDto)
        {
            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == unsubscribeDto.Email && 
                                        s.UnsubscribeToken == unsubscribeDto.UnsubscribeToken);

            if (subscriber == null)
            {
                return new SubscriptionResponseDto
                {
                    Success = false,
                    Message = "Invalid unsubscribe request."
                };
            }

            subscriber.IsActive = false;
            subscriber.UnsubscribedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new SubscriptionResponseDto
            {
                Success = true,
                Message = "You have been successfully unsubscribed from our newsletter."
            };
        }

        public async Task<bool> IsSubscribedAsync(string email)
        {
            return await _context.Subscribers
                .AnyAsync(s => s.Email == email && s.IsActive);
        }

        public async Task<IEnumerable<string>> GetAllActiveSubscribersAsync()
        {
            return await _context.Subscribers
                .Where(s => s.IsActive)
                .Select(s => s.Email)
                .ToListAsync();
        }

        private async Task SendWelcomeEmailAsync(Subscriber subscriber)
        {
            var unsubscribeUrl = $"http://localhost:5246/api/subscription/unsubscribe?token={subscriber.UnsubscribeToken}&email={subscriber.Email}";
            
            var emailBody = $@"
                <h3>Welcome to Our Newsletter!</h3>
                <p>Dear {(string.IsNullOrEmpty(subscriber.Name) ? "Subscriber" : subscriber.Name)},</p>
                <p>Thank you for subscribing to our newsletter! We're excited to have you join our community.</p>
                <p>You'll now receive updates about:</p>
                <ul>
                    <li>Latest blog posts</li>
                    <li>Cybersecurity tips and tricks</li>
                    <li>Industry news and updates</li>
                </ul>
                <p>If you wish to unsubscribe in the future, you can click the link below:</p>
                <p><a href='{unsubscribeUrl}'>Unsubscribe from newsletter</a></p>
                <p>Best regards,<br>The CybersecBlog Team</p>";

            await _emailService.SendEmailAsync(
                subscriber.Email,
                "Welcome to Our Newsletter!",
                emailBody
            );
        }
    }
} 