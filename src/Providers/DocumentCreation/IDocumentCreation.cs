using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation
{
    public interface IDocumentCreation
    {
        EProviderPriority Priority { get; }

        EDocumentType DocumentType { get; }

        byte[] CreateDocument(List<string> FileContent);

        byte[] CreateHtmlDocument(List<string> FileContent, string FormName);

        byte[] CreatePdfDocument(List<string> FileContent, string FormName);
    }
}