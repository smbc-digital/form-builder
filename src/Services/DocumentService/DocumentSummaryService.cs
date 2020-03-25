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
    public interface IDocumentSummaryService
    {
        byte[] GenerateDocument(DocumentSummaryEntity entity);
    }

    public class DocumentSummaryService : IDocumentSummaryService
    {
        private IEnumerable<IDocumentCreation> _textfileProviders;
        private IDocumentCreationHelper _documentCreationHelper;

        public DocumentSummaryService(IDocumentCreationHelper documentCreationHelper, IEnumerable<IDocumentCreation> providers)
        {
            _textfileProviders = providers.Where(_ => _.DocumentType == EDocumentType.Txt);
            _documentCreationHelper = documentCreationHelper;
        }

        public byte[] GenerateDocument(DocumentSummaryEntity entity)
        {
            //return document bytes
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
            var data = _documentCreationHelper.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            //Call required provider
            //How to handle multiple providers of same type
            return _textfileProviders.FirstOrDefault().CreateDocument(data);
        }
    }
}
