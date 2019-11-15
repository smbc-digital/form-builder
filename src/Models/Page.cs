using form_builder.Enum;
using form_builder.Validators;
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
        public string PageURL { get; set; }
        public List<Element> Elements { get; set; }
        public List<Behaviour> Behaviours { get; set; }

        public bool IsValidated { get; set; }

        public bool IsValid
        {
            get
            {
                return !InvalidElements.Any();
            }
        }

        public IEnumerable<Property> InvalidElements
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
        public IEnumerable<Element> ValidatableElements { 
            get
            {
                return Elements.Where(element => element.Type == EElementType.Radio   || 
                                                    element.Type == EElementType.Textarea ||
                                                    element.Type == EElementType.Select ||
                                                    element.Type == EElementType.Textbox ||
                                                    element.Type == EElementType.Checkbox ||
                                                    element.Type == EElementType.Address
                                                    );
            }
        }
        
        public void Validate(Dictionary<string, string> viewModel, IEnumerable<IElementValidator> form_builder)
        {   
            ValidatableElements.ToList()
                .ForEach(element => element.Validate(viewModel, form_builder));

            IsValidated = true;
        }

        public Behaviour GetNextPage(Dictionary<string, string> viewModel)
        {
            if (Behaviours.Count == 1)
            {
                return Behaviours.FirstOrDefault();
            }

            return Behaviours.OrderByDescending(_ => _.Conditions.Count)
                            .Where(_ => _.Conditions.All(x => x.EqualTo == viewModel[x.QuestionId]))
                            .FirstOrDefault();
        }

        public string GetSubmitFormEndpoint(FormAnswers formAnswers)
        {
            var pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitForm);

            if (Behaviours.Count > 1)
            {
                var previousPage = formAnswers.Pages.Where(_ => _.PageUrl == formAnswers.Path)
                    .FirstOrDefault();

                var viewModel = new Dictionary<string, string>();
                previousPage.Answers.ForEach(_ => viewModel.Add(_.QuestionId, _.Response));
                return GetNextPage(viewModel).pageURL;
            }

            return pageSubmitBehaviours.Count == 0 
                ? null 
                : pageSubmitBehaviours.FirstOrDefault().pageURL;
        }

        private List<Behaviour> GetBehavioursByType(EBehaviourType type)
        {
            return Behaviours.Where(_ => _.BehaviourType == type).ToList();
        }
    }
}