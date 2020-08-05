using System;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Factories.Schema;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.DocumentService.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.DocumentService
{
    public interface IDocumentWorkflow
    {
        Task<byte[]> GenerateSummaryDocumentAsync(EDocumentType documentType, Guid id);
    }

    public class DocumentWorkflow : IDocumentWorkflow
    {
        private IDocumentSummaryService _documentSummaryService;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly ISchemaFactory _schemaFactory;

        public DocumentWorkflow(IDocumentSummaryService documentSummaryService, IDistributedCacheWrapper distributedCache, ISchemaFactory schemaFactory, IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
        {
            _documentSummaryService = documentSummaryService;
            _distributedCache = distributedCache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _schemaFactory = schemaFactory;
        }

        public async Task<byte[]> GenerateSummaryDocumentAsync(EDocumentType documentType, Guid id)
        {
            var formData = _distributedCache.GetString($"document-{id.ToString()}");

            if(formData == null)
                throw new DocumentExpiredException($"DocumentWorkflow::GenerateSummaryDocument, Previous answers has expired, unable to generate {documentType.ToString()} document for summary");

            var previousAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            
            var baseForm = await _schemaFactory.Build(previousAnswers.FormName);

            return _documentSummaryService.GenerateDocument(
                new DocumentSummaryEntity{
                    DocumentType = documentType,
                    PreviousAnswers = previousAnswers,
                    FormSchema = baseForm });
        }
    }
}
