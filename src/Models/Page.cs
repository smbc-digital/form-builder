using form_builder.Conditions;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models.Actions;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators;
using Newtonsoft.Json;
using NuGet.Packaging;

namespace form_builder.Models
{
    public class Page
    {
        public Page()
        {
            IsValidated = false;
        }

        public string Title { get; set; }

        public string PageSlug { get; set; }

        public bool DisplayBreadCrumbs { get; set; }

        public bool DisplayOptionalInTitle { get; set; } = false;

        public List<IElement> Elements { get; set; }

        public List<Behaviour> Behaviours { get; set; }

        public List<IncomingValue> IncomingValues { get; set; } = new List<IncomingValue>();

        public bool IsValidated { get; set; }

        public bool HideTitle { get; set; }

        public bool HideBackButton { get; set; }

        public string BannerTitle { get; set; }

        public string LeadingParagraph { get; set; }

        [JsonIgnore] public bool IsValid => !InvalidElements.Any();

        public bool HasIncomingValues => IncomingValues.Any();
        public bool HasIncomingGetValues => IncomingValues.Any(_ => _.HttpActionType.Equals(EHttpActionType.Get));
        public bool HasIncomingPostValues => IncomingValues.Any(_ => _.HttpActionType.Equals(EHttpActionType.Post));

        public List<IAction> PageActions { get; set; } = new List<IAction>();

        public bool HasPageActions => PageActions.Any();

        public bool HasIncomingAction => PageActions.Any();
        public bool HasPageActionsGetValues => PageActions.Any(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Get));
        public bool HasPageActionsPostValues => PageActions.Any(_ => _.Properties.HttpActionType.Equals(EHttpActionType.Post));

        public bool HasDynamicLookupElements => Elements.Any(_ => !string.IsNullOrEmpty(_.Lookup) && _.Lookup.Equals(LookUpConstants.Dynamic));

        public List<Condition> RenderConditions { get; set; } = new List<Condition>();

        public bool HasRenderConditions => RenderConditions.Any();

        [JsonIgnore]
        public IEnumerable<BaseProperty> InvalidElements
        {
            get
            {
                if (IsValidated)
                {
                    IEnumerable<BaseProperty> invalidElements = ValidatableElements.Where(element => !element.IsValid).Select(element => element.Properties);
                    return invalidElements;
                }

                throw new Exception("Model is not validated, please call Validate()");
            }
        }

        public IEnumerable<IElement> ValidatableElements => Elements.Where(element =>
                element.Type.Equals(EElementType.AddAnother) ||
                element.Type.Equals(EElementType.Address) ||
                element.Type.Equals(EElementType.AddressManual) ||
                element.Type.Equals(EElementType.Booking) ||
                element.Type.Equals(EElementType.Checkbox) ||
                element.Type.Equals(EElementType.DatePicker) ||
                element.Type.Equals(EElementType.DateInput) ||
                element.Type.Equals(EElementType.Declaration) ||
                element.Type.Equals(EElementType.FileUpload) ||
                element.Type.Equals(EElementType.Radio) ||
                element.Type.Equals(EElementType.Select) ||
                element.Type.Equals(EElementType.Street) ||
                element.Type.Equals(EElementType.Textarea) ||
                element.Type.Equals(EElementType.Textbox) ||
                element.Type.Equals(EElementType.TimeInput) ||
                element.Type.Equals(EElementType.Map) ||
                element.Type.Equals(EElementType.MultipleFileUpload) ||
                element.Type.Equals(EElementType.Organisation)
        );

        public IEnumerable<IElement> ValidatableElementsConditional => Elements.Where(element =>
            element.Type.Equals(EElementType.Textarea) ||
            element.Type.Equals(EElementType.Select) ||
            element.Type.Equals(EElementType.Textbox) ||
            element.Type.Equals(EElementType.DateInput) ||
            element.Type.Equals(EElementType.TimeInput)
        );

        public void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> validators, FormSchema baseForm)
        {
            ValidatableElements.RemoveUnusedConditionalElements(viewModel)
                .ForEach(element =>
                {
                    element.Validate(viewModel, validators, baseForm);
                });
            IsValidated = true;
        }

        public Behaviour GetNextPage(Dictionary<string, dynamic> viewModel)
        {
            var conditionValidator = new ConditionValidator();

            if (Behaviours.Count.Equals(1))
                return Behaviours.FirstOrDefault();

            foreach (var behaviour in Behaviours.OrderByDescending(_ => _.Conditions.Count))
            {
                var isConditionTrue = false;

                foreach (var condition in behaviour.Conditions)
                {
                    isConditionTrue = conditionValidator.IsValid(condition, viewModel);

                    if (!isConditionTrue)
                        break;
                }

                if (isConditionTrue || !behaviour.Conditions.Any())
                    return behaviour;
            }

            var conditionValuesForDebug = Behaviours.OrderByDescending(_ => _.Conditions.Count)
                .Where(_ => _.Conditions is not null)
                .SelectMany(_ => _.Conditions)
                .Where(_ => !string.IsNullOrEmpty(_.QuestionId))
                .Select(_ => _.QuestionId)
                .Distinct()
                .Select(_ => $"QuestionId: {_} with answer {(viewModel.ContainsKey(_) ? viewModel[_] : "'null'")}")
                .Aggregate((curr, acc) => $"{acc} {curr}");

            throw new ApplicationException($"Page::GetNextPage, There was a problem whilst processing behaviors for page '{PageSlug}', Behaviour Answers and conditions: {conditionValuesForDebug}");
        }

        public bool CheckPageMeetsConditions(Dictionary<string, dynamic> answers)
        {
            var conditionValidator = new ConditionValidator();

            return RenderConditions.Count.Equals(0) ||
                   RenderConditions.All(condition => conditionValidator.IsValid(condition, answers));
        }

        public SubmitSlug GetSubmitFormEndpoint(FormAnswers formAnswers, string environment)
        {
            SubmitSlug submitBehaviour = new();

            var pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitForm);

            if (pageSubmitBehaviours.Count.Equals(0))
                pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitAndPay);

            if (pageSubmitBehaviours.Count.Equals(0))
                pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitAndRedirect);

            if (Behaviours.Count > 1)
            {
                var previousPage = formAnswers.Pages
                    .SelectMany(_ => _.Answers)
                    .ToList();

                var viewModel = new Dictionary<string, dynamic>();
                previousPage.ForEach(_ => viewModel.Add(_.QuestionId, _.Response));
                viewModel.AddRange(formAnswers.AdditionalFormData);
                var foundSubmitBehaviour = GetNextPage(viewModel);

                submitBehaviour = foundSubmitBehaviour.SubmitSlugs
                    .ToList()
                    .FirstOrDefault(x => x.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                if (pageSubmitBehaviours.FirstOrDefault()?.SubmitSlugs.Count is 0)
                {
                    submitBehaviour.URL = pageSubmitBehaviours.FirstOrDefault()?.PageSlug;
                }
                else
                {
                    var behaviour = pageSubmitBehaviours
                        .SelectMany(x => x.SubmitSlugs)
                        .FirstOrDefault(x => x.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));

                    submitBehaviour = behaviour ?? throw new NullReferenceException("Page model::GetSubmitFormEndpoint, No Url supplied for submit form");
                }
            }

            return string.IsNullOrEmpty(submitBehaviour.URL)
                ? throw new NullReferenceException(
                    "Page model::GetSubmitFormEndpoint, No postUrl supplied for submit form")
                : submitBehaviour;
        }

        private List<Behaviour> GetBehavioursByType(EBehaviourType type) => Behaviours.Where(_ => _.BehaviourType.Equals(type)).ToList();

        public string GetPageTitle() => Elements.Any() && HideTitle ? Elements.First().Properties.Label : Title;
    }
}