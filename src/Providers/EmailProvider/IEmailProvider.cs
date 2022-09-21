using System.Net;
using form_builder.Models;

namespace form_builder.Providers.EmailProvider
{
    public interface IEmailProvider
    {
        Task<HttpStatusCode> SendEmail(EmailMessage emailMessage);
    }
}