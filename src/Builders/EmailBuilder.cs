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

            var multipart = new Multipart("mixed")
            {
                new TextPart("html")
                {
                    Text = emailMessage.Body
                }
            };

            if (emailMessage.Attachment is not null)
            {
                var attachment = new MimePart("application", "pdf")
                {
                    Content = new MimeContent(new MemoryStream(emailMessage.Attachment)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = emailMessage.AttachmentName,
                    IsAttachment = true
                };

                multipart.Add(attachment);
            }

            message.Subject = emailMessage.Subject;
            message.Body = multipart;

            return message;
        }

        private static BodyBuilder BuildMessageBody(string bodyContent) => new BodyBuilder
        {
            HtmlBody = bodyContent,
            TextBody = bodyContent
        };
    }
}