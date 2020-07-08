using System.Net;
using System.Threading.Tasks;
using Amazon.SimpleEmail.Model;
using form_builder.Models;

namespace form_builder.Providers.EmailProvider
{
    public interface IEmailProvider
    {
        Task<HttpStatusCode> SendAwsSesEmail(EmailMessage emailMessage);
    }
}