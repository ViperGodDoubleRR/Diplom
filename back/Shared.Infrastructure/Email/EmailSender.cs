using System.Net;
using System.Net.Mail;

using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpClient _client;
        private readonly string _from;

        public SmtpEmailSender(string host, int port, string login, string password, string from)
        {
            _from = from;

            _client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(login, password),
                EnableSsl = true
            };
        }

        public async Task SendConfirmationCodeAsync(string email, string code)
        {
            var message = new MailMessage(_from, email)
            {
                Subject = "Код подтверждения",
                Body = $"Ваш код подтверждения: {code}"
            };

            await _client.SendMailAsync(message);
        }
    }
}
