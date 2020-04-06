using form_builder.Enum;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace form_builder.Helpers.DocumentCreation
{
    public interface IDocumentCreationHelper
    {
        List<string> GenerateQuestionAndAnswersDictionary(FormAnswers formAnswers, FormSchema formSchema);
    }

    public class DocumentCreationHelper : IDocumentCreationHelper
    {
        private readonly IElementMapper _elementMapper;
        public DocumentCreationHelper(IElementMapper elementMapper)
        {
            _elementMapper = elementMapper;
        }
        
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
                var answer = GetValueForQuestion(question, formAnswers, answers);

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

        private string GetValueForQuestion(IElement question, FormAnswers formAnswers, List<Answers> answers)
        {
            object value = null;

            if(question.Type != EElementType.FileUpload){
                value = _elementMapper.GetAnswerValue(question, formAnswers);
            }

            if(value == null && question.Type != EElementType.FileUpload){
                return string.Empty;
            }

            switch (question.Type)
            {
                case EElementType.TimeInput:
                    var convertTime = (TimeSpan)value;
                    var date = DateTime.Today.Add(convertTime);
                    return date.ToString("hh:mm tt");
                case EElementType.DatePicker:
                case EElementType.DateInput:
                    var convertDateTime = (DateTime)value;
                    return convertDateTime.Date.ToString("dd/MM/yyyy");
                case EElementType.Select:
                case EElementType.Radio:
                    var selectValue = question.Properties.Options.FirstOrDefault(_ => _.Value == value.ToString());
                    return selectValue?.Text ?? string.Empty;
                case EElementType.Checkbox:
                    var answerCheckbox = string.Empty;
                    var list = (List<string>)value;
                    list.ForEach((answersCheckbox) =>
                    {
                        answerCheckbox += $" {question.Properties.Options.FirstOrDefault(_ => _.Value == answersCheckbox)?.Text ?? string.Empty},";
                    });
                    return answerCheckbox.EndsWith(",") ? answerCheckbox.Remove(answerCheckbox.Length - 1).Trim() :answerCheckbox.Trim();
                case EElementType.Organisation:
                    var orgValue = (StockportGovUK.NetStandard.Models.Verint.Organisation)value;
                    return !string.IsNullOrEmpty(orgValue.Name) ? orgValue.Name : string.Empty;
                case EElementType.Address:
                    var addressValue = (StockportGovUK.NetStandard.Models.Addresses.Address)value;
                    if(!string.IsNullOrEmpty(addressValue.SelectedAddress)){
                        return addressValue.SelectedAddress;
                    }
                    var manualLine2Text = string.IsNullOrWhiteSpace(addressValue.AddressLine2) ? string.Empty : $",{addressValue.AddressLine2}";
                    return string.IsNullOrWhiteSpace(addressValue.AddressLine1) ? string.Empty : $"{addressValue.AddressLine1}{manualLine2Text},{addressValue.Town},{addressValue.Postcode}";
                case EElementType.Street:
                    var streetValue = (StockportGovUK.NetStandard.Models.Addresses.Address)value;
                    if(string.IsNullOrEmpty(streetValue.PlaceRef) && string.IsNullOrEmpty(streetValue.SelectedAddress)){
                        return string.Empty;
                    }
                    return streetValue.SelectedAddress;
                case EElementType.FileUpload:
                    var fileInput = answers.FirstOrDefault(_ => _.QuestionId == $"{question.Properties.QuestionId}-fileupload")?.Response;
                    return fileInput == null ? string.Empty : Newtonsoft.Json.JsonConvert.DeserializeObject<FileUploadModel>(fileInput.ToString()).TrustedOriginalFileName;
                default:
                    return value.ToString();
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
    }
}