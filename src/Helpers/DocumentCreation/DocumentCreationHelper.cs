using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Helpers.DocumentCreation
{
    public interface IDocumentCreationHelper
    {
        List<string> GenerateQuestionAndAnswersDictionary(FormAnswers formAnswers, FormSchema formSchema);
    }

    public class DocumentCreationHelper : IDocumentCreationHelper
    {
        public List<string> GenerateQuestionAndAnswersDictionary(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new List<string>();
            var filesData = new List<string>();

            var formSchemaQuestions = formSchema.Pages.ToList()
                .Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .ToList();

            var answers = formAnswers.Pages.ToList()
                .SelectMany(_ => _.Answers)
                .ToList();

            formSchemaQuestions.ForEach((question) => {
                var answer = GetValueForQuestion(question, answers);

                if(!string.IsNullOrWhiteSpace(answer)){
                    if(question.Type == EElementType.FileUpload){
                        filesData.Add($"{GetLabelText(question)} {answer}");
                    } else {
                        data.Add($"{GetLabelText(question)} {answer}");
                    }
                }
            });
            
            if(filesData.Any()){
                data.Add(string.Empty);
                data.Add("Files:");
                data.AddRange(filesData);
            }

            return data;
        }

        private string GetValueForQuestion(IElement question, List<Answers> answers){
            switch (question.Type)
            {
                case EElementType.Address:
                    var autoAddress = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-address-description"))?.Response;
                    if(!string.IsNullOrEmpty(autoAddress)){
                        return autoAddress;
                    }
                    var manualLine1 = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-AddressManualAddressLine1"))?.Response;
                    var manualLine2 = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-AddressManualAddressLine2"))?.Response;
                    var town = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-AddressManualAddressTown"))?.Response;
                    var postcode = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-AddressManualAddressPostcode"))?.Response;
                    var manualLine2Text = string.IsNullOrEmpty(manualLine2) ? string.Empty : $",{manualLine2}";
                    return manualLine1 == null && town == null && postcode == null ? string.Empty : $"{manualLine1}{manualLine2Text},{town},{postcode}";
                case EElementType.TimeInput:
                    var min = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-hours"))?.Response;
                    var hour = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-minutes"))?.Response;
                    var amorpm = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-ampm"))?.Response;
                    return min == null && hour == null && amorpm == null ? string.Empty : $"{min}:{hour}{amorpm}";
                case EElementType.DateInput:
                    var day = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-day"))?.Response;
                    var month = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-month"))?.Response;
                    var year = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question, "-year"))?.Response;
                    return day == null && month == null && year == null ? string.Empty : $"{day}/{month}/{year}";
                case EElementType.FileUpload:
                    var fileInput = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question,"-fileupload"));
                    return fileInput == null ? string.Empty : Newtonsoft.Json.JsonConvert.DeserializeObject<FileUploadModel>(fileInput.Response.ToString()).TrustedOriginalFileName;
                case EElementType.Select:
                case EElementType.Checkbox:
                case EElementType.Radio:
                    var answer = answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question));
                    var value = answer != null ? question.Properties.Options.FirstOrDefault(_ => _.Value == answer.Response) : null;
                    return value?.Text ?? string.Empty;
                default:
                    return answers.FirstOrDefault(_ => _.QuestionId == GetQuestionId(question))?.Response;
            }
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
                                                                                : $"{element.Properties.StreetLabel}{optionalLabelText}:";
                case EElementType.Address:
                    return $"{element.Properties.AddressLabel}{optionalLabelText}:";
                default:
                    return $"{element.Properties.Label}{optionalLabelText}:";
            }
        }

        private string GetQuestionId(IElement element, string prefix = ""){

            switch (element.Type)
            {
                case EElementType.DateInput:
                case EElementType.TimeInput:
                case EElementType.FileUpload:
                case EElementType.Address:
                    return $"{element.Properties.QuestionId}{prefix}";
                case EElementType.Organisation:
                    return $"{element.Properties.QuestionId}-organisation-description";
                case EElementType.Street:
                    return $"{element.Properties.QuestionId}-streetaddress-description";
                default:
                    return element.Properties.QuestionId;
            }
        }
    }
}