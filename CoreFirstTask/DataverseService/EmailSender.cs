using System.Net.Mail;
using System.Net;

namespace CoreFirstTask.DataverseService
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(Message message)
        {
            var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:FromEmail"]),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(message.To[0]);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

    public class Message
    {
        public string[] To { get; }
        public string Subject { get; }
        public string Body { get; }

        public Message(string[] to, string subject, string body)
        {
            To = to;
            Subject = subject;
            Body = body;
        }
    }
}
