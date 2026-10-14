namespace form_builder.Workflows.DocumentWorkflow;

public class DocumentWorkflow(IDocumentSummaryService documentSummaryService,
    IDistributedCacheWrapper distributedCache,
    ISchemaFactory schemaFactory,
    IPageHelper pageHelper)
    : IDocumentWorkflow
{
    public async Task<byte[]> GenerateSummaryDocumentAsync(EDocumentType documentType, string id)
    {
        var previousAnswers = pageHelper.GetSavedAnswers(id);

        if (previousAnswers.FormName is null)
        {
            var formData = distributedCache.GetString($"document-{id}");

            if (formData is null)
                throw new DocumentExpiredException($"DocumentWorkflow::GenerateSummaryDocument, Previous answers has expired, unable to generate {documentType.ToString()} document for summary");

            previousAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
        }
        var baseForm = await schemaFactory.Build(previousAnswers.FormName);

        return await documentSummaryService.GenerateDocument(new DocumentSummaryEntity
        {
            DocumentType = documentType,
            PreviousAnswers = previousAnswers,
            FormSchema = baseForm
        });
    }
}