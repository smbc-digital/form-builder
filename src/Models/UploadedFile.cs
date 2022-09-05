namespace form_builder.Models
{
    public class UploadedFile : FormFile
    {
        public UploadedFile(Stream baseStream, long baseStreamOffset, long length, string name, string fileName) : base(baseStream, baseStreamOffset, length, name, fileName)
        {
        }

        public string QuestionId { get; set; }
    }
}