using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.DocumentCreation;

namespace form_builder.Services.DocumentService
{
    public interface IDocumentSummaryService
    {
        void GenerateDocument(EDocumentType documentType, FormAnswers previousAnswers);
    }

    public class DocumentSummaryService : IDocumentSummaryService
    {
        private IEnumerable<IDocumentCreation> _textfileProviders;

        public DocumentSummaryService(IEnumerable<IDocumentCreation> providers)
        {
            _textfileProviders = providers.Where(_ => _.DocumentType == EDocumentType.Txt);
        }

        public void GenerateDocument(EDocumentType documentType, FormAnswers previousAnswers)
        {
            //Generate Document Summary Data
            //Call required provider
            //return document
            throw new System.NotImplementedException(); 
        }
    }
}
