namespace form_builder.Validators;

public class MultipleFileUploadElementValidator(ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache)
    : IElementValidator
{
    public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
    {
        if (!element.Type.Equals(EElementType.MultipleFileUpload))
            return new ValidationResult { IsValid = true };

        if (element.Properties.Optional && viewModel.ContainsKey(ButtonConstants.SUBMIT))
            return new ValidationResult { IsValid = true };

        var key = $"{element.Properties.QuestionId}{FileUploadConstants.SUFFIX}";
        var isValid = false;

        List<DocumentModel> value = viewModel.ContainsKey(key)
            ? viewModel[key]
            : null;

        isValid = !(value is null);

        if (value is null)
        {
            string browserSessionId = sessionHelper.GetBrowserSessionId();
            string formSessionId = $"{baseForm.BaseURL}::{browserSessionId}";
            var cachedAnswers = distributedCache.GetString(formSessionId);

            var convertedAnswers = cachedAnswers is null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var path = viewModel.FirstOrDefault(_ => _.Key.Equals("Path")).Value;

            var pageAnswersString = convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path))?.Answers.FirstOrDefault(_ => _.QuestionId.Equals(key));
            var response = new List<FileUploadModel>();
            if (pageAnswersString is not null)
            {
                response = JsonConvert.DeserializeObject<List<FileUploadModel>>(pageAnswersString.Response.ToString());

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