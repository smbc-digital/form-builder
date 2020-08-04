using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.FileManagement;
using Address = StockportGovUK.NetStandard.Models.Addresses.Address;
using Organisation = StockportGovUK.NetStandard.Models.Verint.Organisation;
using form_builder.Constants;

namespace form_builder.Mappers
{
    public interface IElementMapper
    {
        T GetAnswerValue<T>(IElement element, FormAnswers formAnswers);

        object GetAnswerValue(IElement element, FormAnswers formAnswers);

        string GetAnswerStringValue(IElement question, FormAnswers formAnswers);
    }

    public class ElementMapper : IElementMapper
    {
        private readonly IDistributedCacheWrapper _distributedCacheWrapper;

        public ElementMapper(IDistributedCacheWrapper distributedCacheWrapper)
        {
            _distributedCacheWrapper = distributedCacheWrapper;
        }

        public T GetAnswerValue<T>(IElement element, FormAnswers formAnswers) => (T)GetAnswerValue(element, formAnswers);

        public object GetAnswerValue(IElement element, FormAnswers formAnswers)
        {
            var key = element.Properties.QuestionId;

            switch (element.Type)
            {
                case EElementType.DateInput:
                    return GetDateInputElementValue(key, formAnswers);
                case EElementType.DatePicker:
                    return GetDatePickerElementValue(key, formAnswers);
                case EElementType.Checkbox:
                    return GetCheckboxElementValue(key, formAnswers);
                case EElementType.Declaration:
                    return GetDeclarationElementValue(key, formAnswers);
                case EElementType.TimeInput:
                    return GetTimeElementValue(key, formAnswers);
                case EElementType.Address:
                    return GetAddressElementValue(key, formAnswers);
                case EElementType.Street:
                    return GetStreetElementValue(key, formAnswers);
                case EElementType.Organisation:
                    return GetOrganisationElementValue(key, formAnswers);
                case EElementType.FileUpload:
                    return GetFileUploadElementValue(key, formAnswers);
                default:
                    if (element.Properties.Numeric)
                        return GetNumericElementValue(key, formAnswers);
                    
                    var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                       .Where(_ => _.QuestionId == key)
                       .ToList()
                       .FirstOrDefault();

                    return value?.Response ?? "";
            }
        }

        private object GetDeclarationElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId == key);

            if (value == null || string.IsNullOrEmpty(value.Response))
            {
                return new List<string>();
            }
            var val = value.Response.Split(",");
            return new List<string>(val);
                }

        public string GetAnswerStringValue(IElement question, FormAnswers formAnswers)
        {
            if(question.Type == EElementType.FileUpload){
                var fileInput = formAnswers.Pages
                    .ToList()
                    .SelectMany(_ => _.Answers)
                    .ToList()
                    .FirstOrDefault(_ => _.QuestionId == $"{question.Properties.QuestionId}-fileupload")?.Response;
                return fileInput == null ? string.Empty : JsonConvert.DeserializeObject<FileUploadModel>(fileInput.ToString()).TrustedOriginalFileName;
            }

            object value = GetAnswerValue(question, formAnswers);

            if(value == null)
                return string.Empty;

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
                    list.ForEach((answersCheckbox) => answerCheckbox += $" {question.Properties.Options.FirstOrDefault(_ => _.Value == answersCheckbox)?.Text ?? string.Empty},");
                    return answerCheckbox.EndsWith(",") ? answerCheckbox.Remove(answerCheckbox.Length - 1).Trim() :answerCheckbox.Trim();
                
                case EElementType.Organisation:
                    var orgValue = (Organisation)value;
                    return !string.IsNullOrEmpty(orgValue.Name) ? orgValue.Name : string.Empty;
                case EElementType.Address:
                    var addressValue = (Address)value;
                    if(!string.IsNullOrEmpty(addressValue.SelectedAddress))
                        return addressValue.SelectedAddress;
                    var manualLine2Text = string.IsNullOrWhiteSpace(addressValue.AddressLine2) ? string.Empty : $",{addressValue.AddressLine2}";
                    return string.IsNullOrWhiteSpace(addressValue.AddressLine1) ? string.Empty : $"{addressValue.AddressLine1}{manualLine2Text},{addressValue.Town},{addressValue.Postcode}";
                
                case EElementType.Street:
                    var streetValue = (Address)value;
                    return string.IsNullOrEmpty(streetValue.PlaceRef) && string.IsNullOrEmpty(streetValue.SelectedAddress) ? string.Empty : streetValue.SelectedAddress;
                
                default:
                    return value.ToString();
            }
        }

        private object GetFileUploadElementValue(string key, FormAnswers formAnswers)
        {
            key = $"{key}-fileupload";
            var model = new File();
            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == key)
                .ToList()
                .FirstOrDefault();

            if (value != null && value.Response != null)
            {
                FileUploadModel uploadModel = JsonConvert.DeserializeObject<FileUploadModel>(value.Response.ToString());

                var fileData = _distributedCacheWrapper.GetString(uploadModel.Key);

                if (fileData == null)
                    throw new Exception($"ElementMapper::GetFileUploadElementValue: An error has occurred while attempting to retrieve an uploaded file with key: {uploadModel.Key} from the distributed cache");

                model.Content = fileData;
                model.TrustedOriginalFileName = uploadModel.UntrustedOriginalFileName;
                model.UntrustedOriginalFileName = uploadModel.TrustedOriginalFileName;
                model.KeyName = key;
                
                return model;
            }

            return null;
        }

        private Address GetAddressElementValue(string key, FormAnswers formAnswers)
        {
            var addressObject = new Address();

            var uprnKey = $"{key}{AddressConstants.SELECT_SUFFIX}";
            var addressDescription = $"{key}{AddressConstants.DESCRIPTION_SUFFIX}";
            var manualAddressLineOne = $"{key}-{AddressManualConstants.ADDRESS_LINE_1}";
            var manualAddressLineTwo = $"{key}-{AddressManualConstants.ADDRESS_LINE_2}";
            var manualAddressLineTown = $"{key}-{AddressManualConstants.TOWN}";
            var manualAddressLinePostcode = $"{key}-{AddressManualConstants.POSTCODE}";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId.Equals(manualAddressLineOne) || _.QuestionId.Equals(manualAddressLineTwo) ||
                            _.QuestionId.Equals(manualAddressLineTown) || _.QuestionId.Equals(manualAddressLinePostcode) ||
                            _.QuestionId.Equals(uprnKey) || _.QuestionId.Equals(addressDescription))
                .ToList();

            addressObject.AddressLine1 = value.FirstOrDefault(_ => _.QuestionId.Equals(manualAddressLineOne))?.Response ?? string.Empty;
            addressObject.AddressLine2 = value.FirstOrDefault(_ => _.QuestionId.Equals(manualAddressLineTwo))?.Response ?? string.Empty;
            addressObject.Town = value.FirstOrDefault(_ => _.QuestionId.Equals(manualAddressLineTown))?.Response ?? string.Empty;
            addressObject.Postcode = value.FirstOrDefault(_ => _.QuestionId.Equals(manualAddressLinePostcode))?.Response ?? string.Empty;
            addressObject.PlaceRef = value.FirstOrDefault(_ => _.QuestionId.Equals(uprnKey))?.Response ?? string.Empty;
            addressObject.SelectedAddress = value.FirstOrDefault(_ => _.QuestionId.Equals(addressDescription))?.Response ?? null;

            return addressObject.IsEmpty() ? null : addressObject;
        }
        
        private Address GetStreetElementValue(string key, FormAnswers formAnswers)
        {
            var addressObject = new Address();

            var usrnKey = $"{key}{StreetConstants.SELECT_SUFFIX}";
            var streetDescription = $"{key}{StreetConstants.DESCRIPTION_SUFFIX}";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == usrnKey || _.QuestionId.Equals(streetDescription))
                .ToList();

            addressObject.PlaceRef = value.FirstOrDefault(_ => _.QuestionId.Equals(usrnKey))?.Response ?? string.Empty;
            addressObject.SelectedAddress = value.FirstOrDefault(_ => _.QuestionId.Equals(streetDescription))?.Response ?? null;

            return addressObject.IsEmpty() ? null : addressObject;
        }
        private DateTime? GetDateInputElementValue(string key, FormAnswers formAnswers)
        {
            dynamic dateObject = new ExpandoObject();
            var dateDayKey = $"{key}-day";
            var dateMonthKey = $"{key}-month";
            var dateYearKey = $"{key}-year";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == dateDayKey || _.QuestionId.Equals(dateMonthKey) ||
                            _.QuestionId == dateYearKey)
                .ToList();

            var day = value.FirstOrDefault(_ => _.QuestionId.Equals(dateDayKey))?.Response ?? string.Empty;
            var month = value.FirstOrDefault(_ => _.QuestionId.Equals(dateMonthKey))?.Response ?? string.Empty;
            var year = value.FirstOrDefault(_ => _.QuestionId.Equals(dateYearKey))?.Response ?? string.Empty;

            if (!string.IsNullOrEmpty(day) && !string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
                return DateTime.Parse($"{day}/{month}/{year}");

            return null;
        }
        
        private TimeSpan? GetTimeElementValue(string key, FormAnswers formAnswers)
        {
            dynamic dateObject = new ExpandoObject();
            var timeMinutesKey = $"{key}{TimeConstants.MINUTES_SUFFIX}";
            var timeHoursKey = $"{key}{TimeConstants.HOURS_SUFFIX}";
            var timeAmPmKey = $"{key}{TimeConstants.AM_PM_SUFFIX}";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId.Equals(timeMinutesKey) || _.QuestionId.Equals(timeHoursKey) ||
                            _.QuestionId.Equals(timeAmPmKey))
                .ToList();

            var minutes = value.FirstOrDefault(_ => _.QuestionId.Equals(timeMinutesKey))?.Response ?? string.Empty;
            var hour = value.FirstOrDefault(_ => _.QuestionId.Equals(timeHoursKey))?.Response ?? string.Empty;
            var amPm = value.FirstOrDefault(_ => _.QuestionId.Equals(timeAmPmKey))?.Response ?? string.Empty;

            if (string.IsNullOrEmpty(minutes) || string.IsNullOrEmpty(hour) || string.IsNullOrEmpty(amPm))
                return null;

            var dateTime = DateTime.Parse($"{hour}:{minutes} {amPm}");

            return dateTime.TimeOfDay;

        }

        private Organisation GetOrganisationElementValue(string key, FormAnswers formAnswers)
        {
            var orgObject = new Organisation();
            var organisationKey = $"{key}{OrganisationConstants.SELECT_SUFFIX}";
            var organisationDescriptionKey = $"{key}{OrganisationConstants.DESCRIPTION_SUFFIX}";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId.Equals(organisationKey) || _.QuestionId.Equals(organisationDescriptionKey))
                .ToList();

            orgObject.Reference = value.FirstOrDefault(_ => _.QuestionId.Equals(organisationKey))?.Response ?? string.Empty;
            orgObject.Name = value.FirstOrDefault(_ => _.QuestionId.Equals(organisationDescriptionKey))?.Response ?? string.Empty;

            return orgObject;
        }
        private int? GetNumericElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value == null || string.IsNullOrEmpty(value.Response))
                return null;
            
            return int.Parse(value.Response);
        }
        private DateTime? GetDatePickerElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value == null || string.IsNullOrEmpty(value.Response))
                return null;

            return DateTime.Parse(value.Response);
        }
        private List<string> GetCheckboxElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value == null || string.IsNullOrEmpty(value.Response))
                return new List<string>();
            
            var val = value.Response.Split(",");

            return new List<string>(val);
        }
    }
}