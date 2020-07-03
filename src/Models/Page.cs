using form_builder.Conditions;
using form_builder.Enum;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.Validators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
                                                                element.Type == EElementType.FileUpload
        );


        [JsonIgnore]
        private ConditionValidator _conditionValidator = new ConditionValidator();

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
                    bool isConditionTrue = false;

                    foreach (var condition in behaviour.Conditions)
                    {
                        isConditionTrue = _conditionValidator.IsValid(condition, viewModel);

                        if (!isConditionTrue)
                            break;
                    }

                    if (isConditionTrue || !behaviour.Conditions.Any())
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