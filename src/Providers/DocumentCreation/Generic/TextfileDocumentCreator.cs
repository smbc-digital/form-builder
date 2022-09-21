using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation.Generic
{
    public class TextfileDocumentCreator : IDocumentCreation
    {
        public EProviderPriority Priority => EProviderPriority.High;
        public EDocumentType DocumentType => EDocumentType.Txt;
        public byte[] CreateDocument(List<string> fileContent)
        {
            using (var stream = new MemoryStream())
            {
                var objStreamWriter = new StreamWriter(stream);
                fileContent.ForEach((line) =>
                {
                    objStreamWriter.WriteLine(line);
                });
                objStreamWriter.Flush();
                objStreamWriter.Close();
                return stream.ToArray();
            }
        }
    }
}