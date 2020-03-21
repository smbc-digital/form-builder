namespace form_builder.Models
{
    public class CustomFormFile
    {
        public CustomFormFile(string content, string questionId, long fileSize, string fileName)
        {
            Base64EncodedContent = content;
            UntrustedOriginalFileName = fileName;
            QuestionId = questionId;
            Length = fileSize;
        }

        public string Base64EncodedContent { get; }
        public string UntrustedOriginalFileName { get; }
        public string QuestionId { get; }
        public long Length { get; }
    }
}