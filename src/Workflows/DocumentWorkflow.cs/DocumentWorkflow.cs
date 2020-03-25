using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Models;
using form_builder.Providers.StorageProvider;

namespace form_builder.Services.DocumentService
{
    public interface IDocumentWorkflow
    {
        void GenerateDocument(EDocumentContentType documentContentType, EDocumentType documentType, Guid id);
    }

    public class DocumentWorkflow : IDocumentWorkflow
    {
        private IDocumentSummaryService _documentSummaryService;
        private readonly IDistributedCacheWrapper _distributedCache;

        public DocumentWorkflow(IDocumentSummaryService documentSummaryService, IDistributedCacheWrapper distributedCache)
        {
            _documentSummaryService = documentSummaryService;
            _distributedCache = distributedCache;
        }

        public void GenerateDocument(EDocumentContentType documentContentType, EDocumentType documentType, Guid id)
        {
            switch (documentContentType)
            {
                case EDocumentContentType.Summary:
                    GenerateSummaryDocument(documentType, id);
                    break;
                default:
                    throw new Exception("Unknown EDocumentContentType");
            }
        }

        private void GenerateSummaryDocument(EDocumentType documentType, Guid id)
        {
            var formData = _distributedCache.GetString($"document-{id.ToString()}");

            if(formData == null){
                throw new DocumentExpiredException($"DocumentWorkflow::GenerateSummaryDocument, Previous answers has expired, unable to generate {documentType.ToString()} document for summary");
            }

            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            _documentSummaryService.GenerateDocument(documentType, convertedAnswers);
        }
    }
}
