using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation.Smbc
{
    public class SmbcTextfileDocumentCreator : IDocumentCreation
    {
        public EDocumentType DocumentType => EDocumentType.Txt;

        public EProviderPriority Priority => EProviderPriority.Low;

        public byte[] CreateDocument(List<string> fileContent)
        {
            throw new System.NotImplementedException();
        }

        public byte[] CreateHtmlDocument(List<string> fileContent)
        {
            throw new System.NotImplementedException();
        }

        public byte[] CreatePdfDocument(List<string> fileContent)
        {
            throw new System.NotImplementedException();
        }
    }
}