using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
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

        public DocumentSummaryService(IDocumentCreationHelper documentCreationHelper, IEnumerable<IDocumentCreation> providers)
        {
            _textfileProvider = providers
                                    .Where(_ => _.DocumentType == EDocumentType.Txt)
                                    .OrderByDescending(_ => _.Priority)
                                    .FirstOrDefault();

            _documentCreationHelper = documentCreationHelper;
        }

        public byte[] GenerateDocument(DocumentSummaryEntity entity)
        {
            switch (entity.DocumentType)
            {
                case EDocumentType.Txt:
                    return GenerateTextFile(entity.PreviousAnswers, entity.FormSchema);
                default:
                    throw new Exception("DocumentSummaryService::GenerateDocument, Unknown Document type request for Summary");
            }
        }

        private byte[] GenerateTextFile(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = _documentCreationHelper.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            return _textfileProvider.CreateDocument(data);
        }
    }
}
