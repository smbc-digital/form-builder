namespace form_builder.Models
{
    public class EmailMessage
    {
        public string Subject { get; }

        public string Body { get; }

        public string FromEmail { get; }

        public string ToEmail { get; }

        public string CcEmail { get; }

        public byte[] Attachment { get; }

        public EmailMessage(string subject, string body, string fromEmail, string toEmail, string ccEmail)
        {
            Subject = subject;
            ToEmail = toEmail;
            CcEmail = ccEmail;
        }

        public EmailMessage(string subject, string body, string fromEmail, string toEmail)
        {
            Subject = subject;
            Body = body;
            FromEmail = fromEmail;
            ToEmail = toEmail;
        }

        public EmailMessage(string subject, string body, string fromEmail, string toEmail, byte[] attachment)
        {
            Subject = subject;
            Body = body;
            FromEmail = fromEmail;
            ToEmail = toEmail;
            Attachment = attachment;
        }
    }
}