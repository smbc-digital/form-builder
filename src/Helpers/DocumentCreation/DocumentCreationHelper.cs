using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Helpers.DocumentCreation
{
    public interface IDocumentCreationHelper 
    {
        Dictionary<string, string> GenerateQuestionAndAnswersDictionary(FormAnswers formAnswers, FormSchema formSchema);
    }

    public class DocumentCreationHelper : IDocumentCreationHelper
    {
        public Dictionary<string, string> GenerateQuestionAndAnswersDictionary(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new Dictionary<string, string>();

            var formSchemaAnswers = formSchema.Pages.ToList()
                .SelectMany(_ => _.Elements)
                .ToList();

            var answers = formAnswers.Pages.ToList()
                .SelectMany(_ => _.Answers)
                .ToList();

            //TODO: Replace with GetLabelText and GetValue.
            answers.ForEach((answer) => {
                var questionInFormSchema = formSchemaAnswers.Where(_ => _.Properties.QuestionId == answer.QuestionId)
                                                            .FirstOrDefault();
                                                            
                data.Add(GetLabelText(questionInFormSchema), answer.Response);
            });

            return data;
        }

        private string GetLabelText(IElement element)
        {
            var optionalLabelText = string.Empty;
            if(element.Properties.Optional){
                optionalLabelText = " (optional)";
            }

            switch (element.Type)
            {
                case EElementType.Street:
                    return string.IsNullOrEmpty(element.Properties.StreetLabel) ? $"Search for a street{optionalLabelText}:" 
                                                                                : $"{element.Properties.StreetLabel}{optionalLabelText}";
                case EElementType.Address:
                    return $"{element.Properties.AddressLabel}{optionalLabelText}:";
                default:
                    return $"{element.Properties.Label}{optionalLabelText}:";
            }
        }
    }
}