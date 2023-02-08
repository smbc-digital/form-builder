using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.DocumentCreation;
using form_builder.Models;
using form_builder.Providers.DocumentCreation;
using form_builder.Services.DocumentService.Entities;

namespace form_builder.Services.DocumentService
{
    public class DocumentSummaryService : IDocumentSummaryService
    {
        private readonly IDocumentCreation _textfileProvider;
        private readonly IDocumentCreationHelper _documentCreationHelper;
        private readonly ISchemaFactory _schemaFactory;

        public DocumentSummaryService(IDocumentCreationHelper documentCreationHelper, IEnumerable<IDocumentCreation> providers, ISchemaFactory schemaFactory)
        {
            _textfileProvider = providers
                                    .Where(_ => _.DocumentType.Equals(EDocumentType.Txt))
                                    .OrderByDescending(_ => _.Priority)
                                    .FirstOrDefault();

            _documentCreationHelper = documentCreationHelper;
            _schemaFactory = schemaFactory;
        }

        public async Task<byte[]> GenerateDocument(DocumentSummaryEntity entity)
        {
            var journeyPages = entity.FormSchema.GetReducedPages(entity.PreviousAnswers);
            foreach (var page in journeyPages)
            {
                await _schemaFactory.TransformPage(page, entity.PreviousAnswers);
            }

            return entity.DocumentType switch
            {
                EDocumentType.Txt => await GenerateTextFile(entity.PreviousAnswers, entity.FormSchema),
                EDocumentType.Html => await GenerateHtmlFile(entity.PreviousAnswers, entity.FormSchema),
                EDocumentType.Pdf => await GeneratePdfFile(entity.PreviousAnswers, entity.FormSchema),
                _ => throw new Exception("DocumentSummaryService::GenerateDocument, Unknown Document type request for Summary"),
            };
        }

        private async Task<byte[]> GenerateTextFile(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = await _documentCreationHelper.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            return _textfileProvider.CreateDocument(data);
        }

        private async Task<byte[]> GenerateHtmlFile(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = await _documentCreationHelper.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            return _textfileProvider.CreateHtmlDocument(data);
        }

        private async Task<byte[]> GeneratePdfFile(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = await _documentCreationHelper.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            return _textfileProvider.CreatePdfDocument(data);
        }
    }
}
