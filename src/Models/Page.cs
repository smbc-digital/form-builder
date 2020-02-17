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
                                                               element.Type == EElementType.Address ||
                                                               element.Type == EElementType.AddressManual ||
                                                               element.Type == EElementType.DateInput ||
                                                               element.Type == EElementType.TimeInput ||
                                                               element.Type == EElementType.DatePicker ||
                                                               element.Type == EElementType.Street ||
                                                               element.Type == EElementType.Organisation ||
                                                               element.Type == EElementType.FileUpload
        );

        public void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> form_builder)
        {
            ValidatableElements.ToList().ForEach(element => element.Validate(viewModel, form_builder));
            IsValidated = true;
        }

        public Behaviour GetNextPage(Dictionary<string, string> viewModel)
        {
            if (Behaviours.Count == 1)
            {
                return Behaviours.FirstOrDefault();
            }
            else
            {
                foreach (var behaviour in Behaviours)
                {
                    foreach (var condition in behaviour.Conditions)
                    {
                        if (condition.EqualTo != null)
                        {
                            return Behaviours
                            .OrderByDescending(_ => _.Conditions.Count)
                            .FirstOrDefault(_ => _.Conditions.All(x => x.EqualTo == viewModel[x.QuestionId]));
                        }
                        else
                        //if (condition.CheckboxContains != null)
                        {
                            return Behaviours
                               .OrderByDescending(_ => _.Conditions.Count)
                               .FirstOrDefault(_ => _.Conditions.All(x => viewModel[x.QuestionId].Contains(x.CheckboxContains)));
                        }
                    }
                }
            }
            throw new Exception("Behaviour issues");
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
                    .FirstOrDefault(_ => _.PageSlug == formAnswers.Path);

                var viewModel = new Dictionary<string, dynamic>();
                previousPage.Answers.ForEach(_ => viewModel.Add(_.QuestionId, _.Response));
                submitBehaviour.URL = GetNextPage(viewModel).PageSlug;
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
                        throw new NullReferenceException("HomeController, Submit: No Url supplied for submit form");
                    }
                    submitBehaviour.URL = behaviour.URL;
                    submitBehaviour.AuthToken = behaviour.AuthToken;
                }
            }

            if (string.IsNullOrEmpty(submitBehaviour.URL))
            {
                throw new NullReferenceException("HomeController, Submit: No postUrl supplied for submit form");
            }

            return submitBehaviour;
        }

        private List<Behaviour> GetBehavioursByType(EBehaviourType type) => Behaviours.Where(_ => _.BehaviourType == type).ToList();
    }
}