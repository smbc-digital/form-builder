namespace form_builder.Models;

public class UploadedFile(Stream baseStream, long baseStreamOffset, long length, string name, string fileName)
    : FormFile(baseStream, baseStreamOffset, length, name, fileName)
{
    public string QuestionId { get; set; }
}