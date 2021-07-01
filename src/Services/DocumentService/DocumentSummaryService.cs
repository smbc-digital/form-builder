using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            switch (entity.DocumentType)
            {
                case EDocumentType.Txt:
                    return await GenerateTextFile(entity.PreviousAnswers, entity.FormSchema);
                default:
                    throw new Exception("DocumentSummaryService::GenerateDocument, Unknown Document type request for Summary");
            }
        }

        private async Task<byte[]> GenerateTextFile(FormAnswers formAnswers, FormSchema formSchema)
        {
            var journeyPages = formSchema.GetReducedPages(formAnswers);
            foreach (var page in journeyPages)
            {
                await _schemaFactory.TransformPage(page, formAnswers);
            }

            var data = await _documentCreationHelper.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            return _textfileProvider.CreateDocument(data);
        }
    }
}
