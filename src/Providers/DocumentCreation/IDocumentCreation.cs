using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Providers.DocumentCreation
{
    public interface IDocumentCreation
    {
        EProviderPriority Priority { get; }

        EDocumentType DocumentType { get; }

        byte[] CreateDocument(List<string> fileContent);

        byte[] CreateHtmlDocument(List<string> fileContent, string formName);

        byte[] CreatePdfDocument(List<string> fileContent, string formName);

        byte[] MakeWordAttachment(List<string> fileContent, string formName);
    }
}