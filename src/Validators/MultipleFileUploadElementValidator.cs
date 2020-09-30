using System.Collections.Generic;
using System.Linq;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Validators
{
    public class MultipleFileUploadElementValidator : IElementValidator
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;

        public MultipleFileUploadElementValidator(ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
        {
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
        }

        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if(element.Type != EElementType.MultipleFileUpload)
                return new ValidationResult { IsValid = true };

            if(element.Properties.Optional && viewModel.ContainsKey(ButtonConstants.SUBMIT))
                return new ValidationResult { IsValid = true };

            var key = $"{ element.Properties.QuestionId}{FileUploadConstants.SUFFIX}";
            var isValid = false;
            
            List<DocumentModel> value = viewModel.ContainsKey(key)
                ? viewModel[key]
                : null;

            isValid = !(value is null);

            if (value == null)
            {
                var sessionGuid = _sessionHelper.GetSessionGuid();
                var cachedAnswers = _distributedCache.GetString(sessionGuid);

                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var path = viewModel.FirstOrDefault(_ => _.Key.Equals("Path")).Value;

                var pageAnswersString = convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path))?.Answers.FirstOrDefault(_ => _.QuestionId == key);
                var response = new List<FileUploadModel>();
                if (pageAnswersString != null)
                {
                    response = JsonConvert.DeserializeObject<List<FileUploadModel>>(pageAnswersString.Response.ToString());

                    // Sets a different message if on the subpage and no files selected
                    if (response.Any())
                        isValid = true;
                }
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = ValidationConstants.FILEUPLOAD_EMPTY
            };
        }
    }
}