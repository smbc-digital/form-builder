namespace form_builder.Services.DocumentService;

public class DocumentSummaryService(IDocumentCreationHelper documentCreationHelper, IEnumerable<IDocumentCreation> providers) : IDocumentSummaryService
{
    private readonly IDocumentCreation _textFileProvider = providers
        .Where(_ => _.DocumentType.Equals(EDocumentType.Txt))
        .OrderByDescending(_ => _.Priority)
        .FirstOrDefault();

    public async Task<byte[]> GenerateDocument(DocumentSummaryEntity entity) =>
        entity.DocumentType switch
        {
            EDocumentType.Txt => await GenerateTextFile(entity.PreviousAnswers, entity.FormSchema),
            EDocumentType.Html => await GenerateHtmlFile(entity.PreviousAnswers, entity.FormSchema),
            EDocumentType.Pdf => await GeneratePdfFile(entity.PreviousAnswers, entity.FormSchema),
            _ => throw new Exception("DocumentSummaryService::GenerateDocument, Unknown Document type request for Summary"),
        };

    private async Task<byte[]> GenerateTextFile(FormAnswers formAnswers, FormSchema formSchema)
    {
        var data = await documentCreationHelper.GenerateQuestionAndAnswersList(formAnswers, formSchema);

        return _textFileProvider.CreateDocument(data);
    }

    private async Task<byte[]> GenerateHtmlFile(FormAnswers formAnswers, FormSchema formSchema)
    {
        var data = await documentCreationHelper.GenerateQuestionAndAnswersList(formAnswers, formSchema);

        return _textFileProvider.CreateHtmlDocument(data, formSchema.FormName);
    }

    private async Task<byte[]> GeneratePdfFile(FormAnswers formAnswers, FormSchema formSchema)
    {
        var data = await documentCreationHelper.GenerateQuestionAndAnswersListForPdf(formAnswers, formSchema);

        return _textFileProvider.CreatePdfDocument(data, formSchema.FormName);
    }
}