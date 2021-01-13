using form_builder.Services.DocumentService.Entities;

namespace form_builder.Services.DocumentService
{
    public interface IDocumentSummaryService
    {
        byte[] GenerateDocument(DocumentSummaryEntity entity);
    }
}
