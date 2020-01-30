using form_builder.Enum;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.Validators;
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

        public bool IsValidated { get; set; }

        public bool IsValid => !InvalidElements.Any();

        public IEnumerable<BaseProperty> InvalidElements
        {
            get
            {
                if(IsValidated)
                {
                    return ValidatableElements.Where(element => !element.IsValid).Select(element => element.Properties);
                }
                
                throw new System.Exception("Model is not validated, please call Validate()");
            }
        }
        public IEnumerable<IElement> ValidatableElements =>  Elements.Where(element => element.Type == EElementType.Radio   || 
                                                                element.Type == EElementType.Textarea ||
                                                                element.Type == EElementType.Select ||
                                                                element.Type == EElementType.Textbox ||
                                                                element.Type == EElementType.Checkbox ||
                                                                element.Type == EElementType.Address ||
                                                                element.Type == EElementType.AddressManual ||
                                                                element.Type == EElementType.DateInput ||
                                                                element.Type == EElementType.TimeInput ||
                                                                element.Type == EElementType.DatePicker ||
                                                                element.Type == EElementType.Street ||
                                                                element.Type == EElementType.Organisation);
        
        public void Validate(Dictionary<string, string> viewModel, IEnumerable<IElementValidator> form_builder)
        {   
            ValidatableElements.ToList().ForEach(element => element.Validate(viewModel, form_builder));
            IsValidated = true;
        }

        public Behaviour GetNextPage(Dictionary<string, string> viewModel) => Behaviours.Count == 1
            ? Behaviours.FirstOrDefault()
            : Behaviours
                .OrderByDescending(_ => _.Conditions.Count)
                .FirstOrDefault(_ => _.Conditions.All(x => x.EqualTo == viewModel[x.QuestionId]));

        public string GetSubmitFormEndpoint(FormAnswers formAnswers, string environment)
        {
            var submitBehaviour = string.Empty;

            var pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitForm);
            if (pageSubmitBehaviours.Count == 0)
            {
                pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitAndPay);
            }

            if (Behaviours.Count > 1)
            {
                var previousPage = formAnswers.Pages
                    .FirstOrDefault(_ => _.PageSlug == formAnswers.Path);

                var viewModel = new Dictionary<string, string>();
                previousPage.Answers.ForEach(_ => viewModel.Add(_.QuestionId, _.Response));
                submitBehaviour = GetNextPage(viewModel).PageSlug;
            }
            else
            {
                int index = 0;
                switch (environment.ToLower())
                {
                    case "local":
                        index = 0;
                        break;
                    case "int":
                        index = 1;
                        break;
                    case "qa":
                        index = 2;
                        break;
                    case "staging":
                        index = 3;
                        break;
                    case "prod":
                        index = 4;
                        break;
                    default:
                        index = 0;
                        break;
                }
                submitBehaviour = pageSubmitBehaviours.FirstOrDefault()?.SubmitSlugs[index].URL;
            }

            if (string.IsNullOrEmpty(submitBehaviour))
            {
                throw new NullReferenceException("HomeController, Submit: No postUrl supplied for submit form");
            }

            return submitBehaviour;
        }

        private List<Behaviour> GetBehavioursByType(EBehaviourType type) => Behaviours.Where(_ => _.BehaviourType == type).ToList();
    }
}