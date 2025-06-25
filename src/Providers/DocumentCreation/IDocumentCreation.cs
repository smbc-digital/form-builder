using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation
{
    public interface IDocumentCreation
    {
        EProviderPriority Priority { get; }

        EDocumentType DocumentType { get; }

        byte[] CreateDocument(List<string> fileContent);

        byte[] CreateHtmlDocument(List<string> fileContent, string formName, string answersTitle);

        byte[] CreatePdfDocument(List<string> fileContent, string formName, string answersTitle);
    }
}