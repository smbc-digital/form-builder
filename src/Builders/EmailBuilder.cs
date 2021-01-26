using System;
using System.IO;
using form_builder.Models;
using MimeKit;

namespace form_builder.Builders.Email
{
    public class EmailBuilder
    {
        public MemoryStream BuildMessageToStream(EmailMessage emailMessage)
        {
            var stream = new MemoryStream();
            BuildMessage(emailMessage).WriteTo(stream);

            return stream;
        }

        private static MimeMessage BuildMessage(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            var toEmails = emailMessage.ToEmail.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var email in toEmails)
            {
                message.To.Add(new MailboxAddress(string.Empty, email.Trim()));
            }

            message.From.Add(new MailboxAddress(string.Empty, emailMessage.FromEmail));

            if (!string.IsNullOrEmpty(emailMessage.CcEmail))
                message.Cc.Add(new MailboxAddress(string.Empty, emailMessage.CcEmail));

            message.Subject = emailMessage.Subject;
            message.Body = BuildMessageBody(emailMessage.Body).ToMessageBody();

            return message;
        }

        private static BodyBuilder BuildMessageBody(string bodyContent) => new BodyBuilder
        {
            HtmlBody = bodyContent,
            TextBody = bodyContent
        };
    }
}