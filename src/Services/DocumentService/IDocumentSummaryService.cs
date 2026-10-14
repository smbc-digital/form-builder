namespace form_builder.Services.DocumentService;

public interface IDocumentSummaryService
{
    Task<byte[]> GenerateDocument(DocumentSummaryEntity entity);
}