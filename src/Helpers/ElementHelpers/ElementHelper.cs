using System.Text;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;

namespace form_builder.Helpers.ElementHelpers;

public class ElementHelper : IElementHelper
{
    private readonly IDistributedCacheWrapper _distributedCache;
    private readonly IElementMapper _elementMapper;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ElementHelper(IDistributedCacheWrapper distributedCacheWrapper,
        IElementMapper elementMapper,
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _distributedCache = distributedCacheWrapper;
        _elementMapper = elementMapper;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public string CurrentValue(string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix = "")
    {
        //Todo
        var defaultValue = string.Empty;

        return GetCurrentValueFromSavedAnswers<string>(defaultValue, questionId, viewmodel, answers, suffix);
    }

    public T CurrentValue<T>(string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix = "") where T : new()
    {
        //Todo
        var defaultValue = new T();

        return GetCurrentValueFromSavedAnswers<T>(defaultValue, questionId, viewmodel, answers, suffix);
    }

    private T GetCurrentValueFromSavedAnswers<T>(T defaultValue, string questionId, Dictionary<string, dynamic> viewmodel, FormAnswers answers, string suffix)
    {
        var currentValue = viewmodel.ContainsKey($"{questionId}{suffix}");

        if (!currentValue)
        {
            var storedValue = answers.Pages?.SelectMany(_ => _.Answers);

            if (storedValue is not null)
            {
                var value = storedValue.FirstOrDefault(_ => _.QuestionId.Equals($"{questionId}{suffix}"));
                return value is not null ? (T)value.Response : defaultValue;
            }
            return defaultValue;
        }

        return viewmodel[$"{questionId}{suffix}"];
    }

    public bool CheckForLabel(Element element)
    {
        if (string.IsNullOrEmpty(element.Properties.Label))
            throw new Exception("No label found for element. Cannot render form.");

        return true;
    }

    public bool CheckForQuestionId(Element element)
    {
        if (string.IsNullOrEmpty(element.Properties.QuestionId))
            throw new Exception("No question id found for element. Cannot render form.");

        return true;
    }

    public bool CheckForMaxLength(Element element)
    {
        if (element.Properties.MaxLength < 1)
            throw new Exception("Max Length must be greater than zero. Cannot render form.");

        return true;
    }

    public bool CheckIfLabelAndTextEmpty(Element element)
    {
        if (string.IsNullOrEmpty(element.Properties.Label) && string.IsNullOrEmpty(element.Properties.Text))
            throw new Exception("An inline alert requires either a label or text or both to be present. Both cannot be empty");

        return true;
    }

    public bool CheckForRadioOptions(Element element)
    {
        if (element.Properties.Options is null)
            throw new Exception("A radio element requires options to be present.");

        if (!element.Properties.AllowSingleOption && element.Properties.Options.Count < 2)
            throw new Exception("A radio element requires two or more options to be present.");

        if (element.Properties.AllowSingleOption && element.Properties.Options.Count < 1)
            throw new Exception("A radio element requires one or more options to be present.");

        return true;
    }

    public bool CheckForSelectOptions(Element element)
    {
        if (element.Properties.Options is null || element.Properties.Options.Count <= 1)
            throw new Exception("A select element requires two or more options to be present.");

        return true;
    }

    public bool CheckForCheckBoxListValues(Element element)
    {
        if (element.Properties.Options is null || element.Properties.Options.Count < 1)
            throw new Exception("A checkbox list requires one or more options to be present.");

        return true;
    }

    public bool CheckAllDateRestrictionsAreNotEnabled(Element element)
    {
        if (element.Properties.RestrictCurrentDate && element.Properties.RestrictPastDate && element.Properties.RestrictFutureDate)
            throw new Exception("Cannot set all date restrictions to true");

        return true;
    }

    public void ReSelectPreviousSelectedOptions(Element element)
    {
        foreach (var option in element.Properties.Options)
        {
            if (option.Value.Equals(element.Properties.Value))
            {
                option.Selected = true;
            }
            else
            {
                option.Selected = false;
            }
        }
    }

    public void ReCheckPreviousRadioOptions(Element element)
    {
        foreach (var option in element.Properties.Options)
        {
            if (option.Value.Equals(element.Properties.Value))
            {
                option.Checked = true;
            }
            else
            {
                option.Checked = false;
            }
        }
    }

    public bool CheckForProvider(Element element)
    {
        if (string.IsNullOrEmpty(element.Properties.StreetProvider) && element.Type.Equals(EElementType.Street)
            || string.IsNullOrEmpty(element.Properties.AddressProvider) && element.Type.Equals(EElementType.Address)
            || string.IsNullOrEmpty(element.Properties.OrganisationProvider) && element.Type.Equals(EElementType.Organisation))
            throw new Exception($"A {element.Type} Provider must be present.");

        return true;
    }

    public object GetFormDataValue(string cacheKey, string key)
    {
        var formData = _distributedCache.GetString(cacheKey);
        var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

        if (!string.IsNullOrEmpty(formData))
            convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

        return convertedAnswers.FormData.ContainsKey(key) ? convertedAnswers.FormData.GetValueOrDefault(key) : string.Empty;
    }

    public FormAnswers GetFormData(string cacheKey)
    {
        var formData = _distributedCache.GetString(cacheKey);
        var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

        if (!string.IsNullOrEmpty(formData))
            convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

        return convertedAnswers;
    }

    public async Task<List<PageSummary>> GenerateQuestionAndAnswersList(string cacheKey, FormSchema formSchema)
    {
        var formAnswers = GetFormData(cacheKey);
        var reducedAnswers = formAnswers.GetReducedAnswers(formSchema);
        var formSummary = new List<PageSummary>();

        foreach (var page in formSchema.Pages.ToList())
        {
            var formSchemaQuestions = new List<IElement>();

            if (page.Elements.Any(_ => _.Type.Equals(EElementType.AddAnother)))
            {
                List<PageSummary> listOfPageSummary = new();
                var addAnotherElement = page.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
                var currentIncrement = GetAddAnotherNumberOfFieldsets(addAnotherElement, formAnswers);
                for (var i = 1; i <= currentIncrement; i++)
                {
                    var addAnotherPageSummary = new PageSummary
                    {
                        PageTitle = addAnotherElement.GetLabelText(page.Title),
                        PageSlug = page.PageSlug,
                        PageSummaryId = $"{page.PageSlug}-{addAnotherElement.Properties.QuestionId}-{i}"
                    };

                    var listOfNestedElements = page.ValidatableElements.Where(_ => _.Properties.QuestionId.Contains($":{i}:")).ToList();
                    addAnotherPageSummary.Answers = await GenerateSummaryAnswers(listOfNestedElements, page, formAnswers, false);
                    listOfPageSummary.Add(addAnotherPageSummary);
                }

                formSummary.AddRange(listOfPageSummary);
            }

            var pageSummary = new PageSummary
            {
                PageTitle = page.Title,
                PageSlug = page.PageSlug,
                PageSummaryId = page.PageSlug
            };

            formSchemaQuestions = page.ValidatableElements
                .Where(_ => _ is not null)
                .ToList();

            if (!formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug.Equals(page.PageSlug)).Select(p => p).Any())
                continue;

            pageSummary.Answers = await GenerateSummaryAnswers(formSchemaQuestions, page, formAnswers, true);

            formSummary.Add(pageSummary);
        }

        return formSummary;
    }

    public int GetAddAnotherNumberOfFieldsets(IElement addAnotherElement, FormAnswers formAnswers)
    {
        if (formAnswers.FormData.Any())
        {
            var formDataIncrementKey = $"{AddAnotherConstants.IncrementKeyPrefix}{addAnotherElement.Properties.QuestionId}";
            return formAnswers.FormData.ContainsKey(formDataIncrementKey)
                ? int.Parse(formAnswers.FormData.GetValueOrDefault(formDataIncrementKey).ToString())
                : 0;
        }

        return 0;
    }

    public async Task<Dictionary<string, string>> GenerateSummaryAnswers(List<IElement> formSchemaQuestions, Page page, FormAnswers formAnswers, bool ignoreDynamicallyGeneratedElements)
    {
        SummaryDictionaryBuilder summaryBuilder = new();

        foreach (var element in formSchemaQuestions)
        {
            if (element.Type.Equals(EElementType.AddAnother) || (element.Properties.IsDynamicallyGeneratedElement && ignoreDynamicallyGeneratedElements))
                continue;

            var answer = await _elementMapper.GetAnswerStringValue(element, formAnswers);
            var summaryLabelText = element.GetLabelText(page.Title);

            summaryBuilder.Add(summaryLabelText, answer, element.Type);
        };

        return summaryBuilder.Build();
    }

    public string GenerateDocumentUploadUrl(Element element, FormSchema formSchema, FormAnswers formAnswers)
    {
        var urlOrigin = $"https://{_httpContextAccessor.HttpContext.Request.Host}/";
        var urlPath = $"{formSchema.BaseURL}/{FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH}{SystemConstants.CaseReferenceQueryString}{Convert.ToBase64String(Encoding.ASCII.GetBytes(formAnswers.CaseReference))}";

        return $"{urlOrigin}{urlPath}";
    }

    public void OrderOptionsAlphabetically(Element element)
    {
        if (element.Properties.OrderOptionsAlphabetically)
            element.Properties.Options.Sort((x, y) => x.Text.CompareTo(y.Text));
    }
}