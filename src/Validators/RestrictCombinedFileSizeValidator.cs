using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Validators
{
    public class RestrictCombinedFileSizeValidator : IElementValidator
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;

        public RestrictCombinedFileSizeValidator(ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
        {
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
        }

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.MultipleFileUpload))
                return new ValidationResult { IsValid = true };

            var key = $"{element.Properties.QuestionId}{FileUploadConstants.SUFFIX}";

            if (!viewModel.ContainsKey(key))
                return new ValidationResult { IsValid = true };

            List<DocumentModel> documentModel = viewModel[key];

            if (documentModel is null)
                return new ValidationResult { IsValid = true };

            var maxCombinedFileSize = element.Properties.MaxCombinedFileSize > 0 ? element.Properties.MaxCombinedFileSize * SystemConstants.OneMBInBinaryBytes : SystemConstants.DefaultMaxCombinedFileSize;

            string browserSessionId = _sessionHelper.GetBrowserSessionId();
            string formSessionId = $"{baseForm.BaseURL}::{browserSessionId}";
            var cachedAnswers = _distributedCache.GetString(formSessionId);

            var convertedAnswers = cachedAnswers is null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var path = viewModel.FirstOrDefault(_ => _.Key.Equals("Path")).Value;

            var pageAnswersString = convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path))?.Answers.FirstOrDefault(_ => _.QuestionId.Equals(key));
            List<FileUploadModel> response = new();
            if (pageAnswersString is not null)
            {
                response = JsonConvert.DeserializeObject<List<FileUploadModel>>(pageAnswersString.Response.ToString());
            }

            if (documentModel.Sum(_ => _.FileSize) + response?.Sum(_ => _.FileSize) < maxCombinedFileSize)
                return new ValidationResult { IsValid = true };

            return new ValidationResult
            {
                IsValid = false,
                Message = $"The total size of all your added files must not be more than {maxCombinedFileSize.ToReadableMaxFileSize()}MB"
            };
        }
    }
}
