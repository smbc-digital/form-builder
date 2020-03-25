using System;
using System.Threading.Tasks;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.DocumentService
{
    public interface IDocumentWorkflow
    {
        Task GenerateDocument(EDocumentContentType documentContentType, EDocumentType documentType, Guid id);
    }

    public class DocumentWorkflow : IDocumentWorkflow
    {
        private IDocumentSummaryService _documentSummaryService;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DistributedCacheExpirationConfiguration _distrbutedCacheExpirationConfiguration;
        private readonly ICache _cache;

        public DocumentWorkflow(IDocumentSummaryService documentSummaryService, IDistributedCacheWrapper distributedCache, ICache cache, IOptions<DistributedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration)
        {
            _documentSummaryService = documentSummaryService;
            _distributedCache = distributedCache;
            _distrbutedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
            _cache = cache;
        }

        public async Task GenerateDocument(EDocumentContentType documentContentType, EDocumentType documentType, Guid id)
        {
            switch (documentContentType)
            {
                case EDocumentContentType.Summary:
                    await GenerateSummaryDocumentAsync(documentType, id);
                    break;
                default:
                    throw new Exception("Unknown EDocumentContentType");
            }
        }

        private async Task GenerateSummaryDocumentAsync(EDocumentType documentType, Guid id)
        {
            var formData = _distributedCache.GetString($"document-{id.ToString()}");

            if(formData == null){
                throw new DocumentExpiredException($"DocumentWorkflow::GenerateSummaryDocument, Previous answers has expired, unable to generate {documentType.ToString()} document for summary");
            }

            var previousAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            
            var baseForm = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(previousAnswers.FormName, _distrbutedCacheExpirationConfiguration.FormJson, ESchemaType.FormJson);

            _documentSummaryService.GenerateDocument(documentType, previousAnswers, baseForm);
        }
    }
}
