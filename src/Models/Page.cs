using form_builder.Enum;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Conditions;

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

        public List<IElement> Elements { get; set; }

        public List<Behaviour> Behaviours { get; set; }

        public List<IncomingValue> IncomingValues { get;set; } = new List<IncomingValue>();

        public bool IsValidated { get; set; }

        public bool HideTitle { get; set; }

        public bool HideBackButton { get; set; }

        public string BannerTitle { get; set; }

        public string LeadingParagraph { get; set; }

        [JsonIgnore]
        public bool IsValid => !InvalidElements.Any();

        public bool HasIncomingValues => IncomingValues.Any();

        public List<PageAction> PageActions { get; set; } = new List<PageAction>();

        public bool HasPageActions => PageActions.Any();

        [JsonIgnore]
        public IEnumerable<BaseProperty> InvalidElements
        {
            get
            {
                if (IsValidated)
                {
                    return ValidatableElements.Where(element => !element.IsValid).Select(element => element.Properties);
                }

                throw new System.Exception("Model is not validated, please call Validate()");
            }
        }

        public IEnumerable<IElement> ValidatableElements => Elements.Where(element => element.Type == EElementType.Radio ||
                                                                element.Type == EElementType.Textarea ||
                                                                element.Type == EElementType.Select ||
                                                                element.Type == EElementType.Textbox ||
                                                                element.Type == EElementType.Checkbox ||
                                                                element.Type == EElementType.Declaration ||
                                                                element.Type == EElementType.Address ||
                                                                element.Type == EElementType.AddressManual ||
                                                                element.Type == EElementType.DateInput ||
                                                                element.Type == EElementType.TimeInput ||
                                                                element.Type == EElementType.DatePicker ||
                                                                element.Type == EElementType.Street ||
                                                                element.Type == EElementType.Organisation ||
                                                                element.Type == EElementType.FileUpload ||
                                                                element.Type == EElementType.Map
        );

        public void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> form_builder)
        {
            ValidatableElements.ToList().ForEach(element => element.Validate(viewModel, form_builder));
            IsValidated = true;
        }

        public Behaviour GetNextPage(Dictionary<string, dynamic> viewModel)
        {
            if (Behaviours.Count == 1)
            {
                return Behaviours.FirstOrDefault();
            }
            else
            {
                foreach (var behaviour in Behaviours.OrderByDescending(_ => _.Conditions.Count))
                {
                    var equalToConditions = behaviour.Conditions.Where(x => !string.IsNullOrEmpty(x.EqualTo));
                    var checkBoxContainsConditions = behaviour.Conditions.Where(x => !string.IsNullOrEmpty(x.CheckboxContains));
                    var dateIsBeforeConditions = behaviour.Conditions.Where(x => x.IsBefore != null);
                    var dateIsAfterConditions = behaviour.Conditions.Where(x => x.IsAfter != null);
                    var isNullOrEmpty = behaviour.Conditions.Where(x => x.IsNullOrEmpty != null);

                    var equalToValid = !equalToConditions.Any();
                    var checkBoxContainsValid = !checkBoxContainsConditions.Any();
                    var dateIsBeforeValid = !dateIsBeforeConditions.Any();
                    var dateIsAfterValid = !dateIsAfterConditions.Any();
                    var isNullOrEmptyValid = !isNullOrEmpty.Any();

                    if(equalToConditions.Any())
                        equalToValid = equalToConditions.All(x => x.EqualTo.Equals(viewModel.ContainsKey(x.QuestionId) ? viewModel[x.QuestionId] : string.Empty));

                    if(checkBoxContainsConditions.Any())
                        checkBoxContainsValid = checkBoxContainsConditions.All(x => viewModel[x.QuestionId].Contains(x.CheckboxContains));
                        
                    if(dateIsBeforeConditions.Any())
                        dateIsBeforeValid = dateIsBeforeConditions.All(x => DateComparator.DateIsBefore(x, viewModel));
                        
                    if(dateIsAfterConditions.Any())
                        dateIsAfterValid = dateIsAfterConditions.All(x => DateComparator.DateIsAfter(x, viewModel));  

                    if (isNullOrEmpty.Any())
                        isNullOrEmptyValid = isNullOrEmpty.All(x => string.IsNullOrEmpty(viewModel[x.QuestionId]) == x.IsNullOrEmpty);

                    if (equalToValid && checkBoxContainsValid && dateIsBeforeValid && dateIsAfterValid && isNullOrEmptyValid)
                        return behaviour;
                }
            }

            throw new Exception("Page model, There was a problem whilst processing behaviors");
        }

        public SubmitSlug GetSubmitFormEndpoint(FormAnswers formAnswers, string environment)
        {
            var submitBehaviour = new SubmitSlug();

            var pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitForm);
            if (pageSubmitBehaviours.Count == 0)
            {
                pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitAndPay);
            }

            if (Behaviours.Count > 1)
            {
                var previousPage = formAnswers.Pages
                    .SelectMany(_ => _.Answers)
                    .ToList();

                var viewModel = new Dictionary<string, dynamic>();
                previousPage.ForEach(_ => viewModel.Add(_.QuestionId, _.Response));
                var foundSubmitBehaviour = GetNextPage(viewModel);

                submitBehaviour = foundSubmitBehaviour.SubmitSlugs.ToList()
                            .Where(x => x.Environment.ToLower() == environment.ToLower())
                            .FirstOrDefault();
            }
            else
            {
                if (pageSubmitBehaviours.FirstOrDefault()?.SubmitSlugs.Count == 0)
                {
                    submitBehaviour.URL = pageSubmitBehaviours.FirstOrDefault()?.PageSlug;
                }
                else
                {
                    var behaviour = pageSubmitBehaviours.SelectMany(x => x.SubmitSlugs)
                            .Where(x => x.Environment.ToLower() == environment.ToLower())
                            .FirstOrDefault();

                    if (behaviour == null)
                    {
                        throw new NullReferenceException("Page model, Submit: No Url supplied for submit form");
                    }

                    submitBehaviour = behaviour;
                }
            }

            if (string.IsNullOrEmpty(submitBehaviour.URL))
            {
                throw new NullReferenceException("Page model, Submit: No postUrl supplied for submit form");
            }

            return submitBehaviour;
        }

        private List<Behaviour> GetBehavioursByType(EBehaviourType type) => Behaviours.Where(_ => _.BehaviourType == type).ToList();

        public string GetPageTitle()
        {
            if (Elements.Any() && HideTitle)
            {
                return Elements.First().Properties.Label;
            }

            return Title;
        }
    }
}