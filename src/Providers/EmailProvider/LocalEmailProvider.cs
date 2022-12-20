using StockportGovUK.NetStandard.Gateways.MailingService;
using System.Net;
using System.Net.Mail;
using form_builder.Models;

namespace form_builder.Providers.EmailProvider
{
    public class LocalEmailProvider : IEmailProvider
    {

        public LocalEmailProvider()
        {

        }

        public Task<HttpStatusCode> SendEmail(EmailMessage emailMessage)
        {

            using (var client = new SmtpClient("scnmailfilter1"))
            {
                MailMessage mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(emailMessage.FromEmail);
                mailMessage.Body = emailMessage.Body;
                mailMessage.Subject = emailMessage.Subject;
                mailMessage.IsBodyHtml = false;

                foreach (var address in emailMessage.ToEmail.Split(","))
                {
                    mailMessage.To.Add(new MailAddress(address));
                }

                client.Send(mailMessage);
            }

            return Task.FromResult(HttpStatusCode.OK);
        }
    }
    
}
