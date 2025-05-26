using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BlogBackend.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public EmailService(IConfiguration configuration)
        {
            Console.WriteLine("Debug - Initializing EmailService");
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(configuration["EmailSettings:Port"]);
            _senderEmail = configuration["EmailSettings:Username"];
            _senderPassword = configuration["EmailSettings:Password"];

            Console.WriteLine($"Debug - SMTP Server: {_smtpServer}");
            Console.WriteLine($"Debug - SMTP Port: {_smtpPort}");
            Console.WriteLine($"Debug - Sender Email: {_senderEmail}");
            Console.WriteLine("Debug - Password present: {0}", !string.IsNullOrEmpty(_senderPassword));

            if (string.IsNullOrEmpty(_senderPassword))
            {
                Console.WriteLine("Debug - Getting password from environment variable");
                _senderPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                Console.WriteLine("Debug - Environment variable password present: {0}", !string.IsNullOrEmpty(_senderPassword));
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            Console.WriteLine($"Debug - SendEmailAsync called for {toEmail}");
            
            if (string.IsNullOrEmpty(toEmail))
                throw new ArgumentException("Recipient email address cannot be null or empty.", nameof(toEmail));

            if (string.IsNullOrEmpty(_senderPassword))
            {
                Console.WriteLine("Error - Email password is not configured");
                throw new InvalidOperationException("Email service is not properly configured: Missing password");
            }

            try
            {
                Console.WriteLine($"Debug - Creating SmtpClient for {_smtpServer}:{_smtpPort}");
                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                Console.WriteLine("Debug - Creating MailMessage");
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, "CybersecBlog"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                Console.WriteLine("Debug - About to send email");
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("Debug - Email sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug - Email sending failed: {ex.Message}");
                Console.WriteLine($"Debug - Stack trace: {ex.StackTrace}");
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        private string CreateEmailTemplate(string subject, string content)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            margin: 0;
                            padding: 0;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                        }}
                        .header {{
                            background-color: #2c3e50;
                            color: white;
                            padding: 20px;
                            text-align: center;
                            border-radius: 5px 5px 0 0;
                        }}
                        .content {{
                            background-color: #ffffff;
                            padding: 20px;
                            border: 1px solid #e9e9e9;
                            border-radius: 0 0 5px 5px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 10px 20px;
                            background-color: #3498db;
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin-top: 15px;
                        }}
                        .footer {{
                            margin-top: 20px;
                            text-align: center;
                            color: #666;
                            font-size: 12px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='header'>
                            <h2>{subject}</h2>
                        </div>
                        <div class='content'>
                            {content}
                        </div>
                        <div class='footer'>
                            <p>This is an automated message from CybersecBlog. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}