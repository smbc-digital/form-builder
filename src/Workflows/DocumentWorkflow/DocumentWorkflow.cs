using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using Newtonsoft.Json;

namespace form_builder.Workflows.DocumentWorkflow
{
	public class DocumentWorkflow : IDocumentWorkflow
	{
		private IDocumentSummaryService _documentSummaryService;
		private readonly IDistributedCacheWrapper _distributedCache;
		private readonly IPageHelper _pageHelper;
		private readonly ISchemaFactory _schemaFactory;

		public DocumentWorkflow(IDocumentSummaryService documentSummaryService, IDistributedCacheWrapper distributedCache, ISchemaFactory schemaFactory, IPageHelper pageHelper)
		{
			_documentSummaryService = documentSummaryService;
			_distributedCache = distributedCache;
			_schemaFactory = schemaFactory;
			_pageHelper = pageHelper;
		}

		public async Task<byte[]> GenerateSummaryDocumentAsync(EDocumentType documentType, string id)
		{
			var previousAnswers = _pageHelper.GetSavedAnswers(id);

			if (previousAnswers.FormName is null)
			{
				var formData = _distributedCache.GetString($"document-{id}");

				if (formData is null)
					throw new DocumentExpiredException($"DocumentWorkflow::GenerateSummaryDocument, Previous answers has expired, unable to generate {documentType.ToString()} document for summary");

				previousAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
			}
			var baseForm = await _schemaFactory.Build(previousAnswers.FormName);

			return await _documentSummaryService.GenerateDocument(new DocumentSummaryEntity
			{
				DocumentType = documentType,
				PreviousAnswers = previousAnswers,
				FormSchema = baseForm
			});
		}
	}
}