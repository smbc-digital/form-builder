namespace form_builder.Models;

public class CustomFormFile(string content, string questionId, long fileSize, string fileName)
{
    public string Base64EncodedContent { get; } = content;

    public string UntrustedOriginalFileName { get; } = fileName;

    public string QuestionId { get; } = questionId;

    public long Length { get; } = fileSize;
}