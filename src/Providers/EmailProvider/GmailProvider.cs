using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Providers.EmailProvider
{
    public class GmailProvider : IEmailProvider
    {
        public async Task<HttpStatusCode> SendEmail(EmailMessage emailMessage)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailMessage.FromEmail),
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = true
            };

            foreach (var email in emailMessage.ToEmail.Split(","))
            {
                if (!string.IsNullOrEmpty(email))
                    mailMessage.To.Add(email);
            }

            var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("", ""),
                EnableSsl = true
            };

            client.Send(mailMessage);

            return await Task.FromResult(HttpStatusCode.OK);
        }
    }
}