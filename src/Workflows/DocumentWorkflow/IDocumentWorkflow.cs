namespace form_builder.Workflows.DocumentWorkflow;

public interface IDocumentWorkflow
{
    Task<byte[]> GenerateSummaryDocumentAsync(EDocumentType documentType, string id);
}