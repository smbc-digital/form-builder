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

        public List<StockportGovUK.NetStandard.Gateways.Models.FileManagement.File> FileUploads { get; set; }

        public string AttachmentName { get; set; }

        public EmailMessage() { }

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

        public EmailMessage(string subject, string body, string fromEmail, string toEmail, List<StockportGovUK.NetStandard.Gateways.Models.FileManagement.File> fileUploads)
        {
            Subject = subject;
            Body = body;
            FromEmail = fromEmail;
            ToEmail = toEmail;
            FileUploads = fileUploads;
        }

        public EmailMessage(string subject, string body, string fromEmail, string toEmail, byte[] attachment, string attachmentName)
        {
            Subject = subject;
            Body = body;
            FromEmail = fromEmail;
            ToEmail = toEmail;
            Attachment = attachment;
            AttachmentName = attachmentName;
        }

        public EmailMessage(string subject, string body, string fromEmail, string toEmail, byte[] attachment, string attachmentName, List<StockportGovUK.NetStandard.Gateways.Models.FileManagement.File> fileUploads)
        {
            Subject = subject;
            Body = body;
            FromEmail = fromEmail;
            ToEmail = toEmail;
            Attachment = attachment;
            AttachmentName = attachmentName;
            FileUploads = fileUploads;
        }
    }
}