using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.Session;
using form_builder.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace form_builder.Services.ActionService
{
    public interface IActionService
    {
        Task Process(FormSchema baseForm);
    }

    public class ActionService : IActionService
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCache _distributedCache;

        public ActionService(ISessionHelper sessionHelper, IDistributedCache distributedCache)
        {
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
        }
        public async Task Process(FormSchema baseForm)
        {
            // Do stuff here
            try
            {
                var sessionGuid = _sessionHelper.GetSessionGuid();

                if (string.IsNullOrEmpty(sessionGuid))
                {
                    throw new Exception("ActionService::Process: Session has expired");
                }

                var formData = _distributedCache.GetString(sessionGuid);
                var formAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
                foreach (var action in baseForm.FormActions)
                {
                    switch (action.Type)
                    {
                        case EFormActionType.UserEmail: 
                            SendUserEmail(action, formAnswers);
                            break;

                        default: break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void SendUserEmail(FormAction action, FormAnswers formAnswers)
        {
            var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("", ""),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(action.Properties.From),
                Subject = action.Properties.Subject,
                Body = action.Properties.Content,
                IsBodyHtml = true
            };

            mailMessage.To.Add(action.Properties.To);

            try
            {
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}