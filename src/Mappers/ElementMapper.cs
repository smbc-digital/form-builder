using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.FileManagement;
using Microsoft.Extensions.Logging;
using Address = StockportGovUK.NetStandard.Models.Addresses.Address;

namespace form_builder.Mappers
{
    public class ElementMapper : IElementMapper
    {
        private readonly IDistributedCacheWrapper _distributedCacheWrapper;
        private readonly ILogger<ElementMapper> _logger;

        public ElementMapper(ILogger<ElementMapper> logger,IDistributedCacheWrapper distributedCacheWrapper)
        {
            _distributedCacheWrapper = distributedCacheWrapper;
            _logger = logger;
        }

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
                    {
                        return GetNumericElementValue(key, formAnswers);
                    }
                    var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                       .Where(_ => _.QuestionId == key)
                       .ToList()
                       .FirstOrDefault();

                    return value?.Response ?? "";
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
                {
                    throw new Exception($"ElementMapper::GetFileUploadElementValue: An error has occurred while attempting to retrieve an uploaded file with key: {uploadModel.Key} from the distributed cache");
                };

                model.Content = fileData;
                model.TrustedOriginalFileName = uploadModel.UntrustedOriginalFileName;
                model.UntrustedOriginalFileName = uploadModel.TrustedOriginalFileName;
                
                return model;
            }
            return null;
        }

        private Address GetAddressElementValue(string key, FormAnswers formAnswers)
        {
            var addressObject = new Address();

            var urpnKey = $"{key}-address";
            var manualAddressLineOne = $"{key}-AddressManualAddressLine1";
            var manualAddressLineTwo = $"{key}-AddressManualAddressLine2";
            var manualAddressLineTown = $"{key}-AddressManualAddressTown";
            var manualAddressLinePostcode = $"{key}-AddressManualAddressPostcode";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == manualAddressLineOne || _.QuestionId == manualAddressLineTwo ||
                            _.QuestionId == manualAddressLineTown || _.QuestionId == manualAddressLinePostcode ||
                            _.QuestionId == urpnKey)
                .ToList();

            addressObject.AddressLine1 = value.FirstOrDefault(_ => _.QuestionId == manualAddressLineOne)?.Response ?? string.Empty;
            addressObject.AddressLine2 = value.FirstOrDefault(_ => _.QuestionId == manualAddressLineTwo)?.Response ?? string.Empty;
            addressObject.Town = value.FirstOrDefault(_ => _.QuestionId == manualAddressLineTown)?.Response ?? string.Empty;
            addressObject.Postcode = value.FirstOrDefault(_ => _.QuestionId == manualAddressLinePostcode)?.Response ?? string.Empty;
            addressObject.PlaceRef = value.FirstOrDefault(_ => _.QuestionId == urpnKey)?.Response ?? string.Empty;

            return addressObject;

        }
        private Address GetStreetElementValue(string key, FormAnswers formAnswers)
        {
            var addressObject = new Address();

            var uspnKey = $"{key}-streetaddress";
            var streetDescription = $"{key}-streetaddress-description";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == uspnKey || _.QuestionId == streetDescription)
                .ToList();

            addressObject.PlaceRef = value.FirstOrDefault(_ => _.QuestionId == uspnKey)?.Response ?? string.Empty;
            addressObject.SelectedAddress = value.FirstOrDefault(_ => _.QuestionId == streetDescription)?.Response ?? string.Empty;

            return addressObject;
        }
        private DateTime? GetDateInputElementValue(string key, FormAnswers formAnswers)
        {
            dynamic dateObject = new ExpandoObject();
            var dateDayKey = $"{key}-day";
            var dateMonthKey = $"{key}-month";
            var dateYearKey = $"{key}-year";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == dateDayKey || _.QuestionId == dateMonthKey ||
                            _.QuestionId == dateYearKey)
                .ToList();

            var day = value.FirstOrDefault(_ => _.QuestionId == dateDayKey)?.Response ?? string.Empty;
            var month = value.FirstOrDefault(_ => _.QuestionId == dateMonthKey)?.Response ?? string.Empty;
            var year = value.FirstOrDefault(_ => _.QuestionId == dateYearKey)?.Response ?? string.Empty;

            if (!string.IsNullOrEmpty(day) && !string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
            {
                return DateTime.Parse($"{day}/{month}/{year}");
            }

            return null;
        }
        private TimeSpan? GetTimeElementValue(string key, FormAnswers formAnswers)
        {
            dynamic dateObject = new ExpandoObject();
            var timeMinutesKey = $"{key}-minutes";
            var timeHoursKey = $"{key}-hours";
            var timeAmPmKey = $"{key}-ampm";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == timeMinutesKey || _.QuestionId == timeHoursKey ||
                            _.QuestionId == timeAmPmKey)
                .ToList();

            var minutes = value.FirstOrDefault(_ => _.QuestionId == timeMinutesKey)?.Response ?? string.Empty;
            var hour = value.FirstOrDefault(_ => _.QuestionId == timeHoursKey)?.Response ?? string.Empty;
            var amPm = value.FirstOrDefault(_ => _.QuestionId == timeAmPmKey)?.Response ?? string.Empty;

            if (!string.IsNullOrEmpty(minutes) && !string.IsNullOrEmpty(hour) && !string.IsNullOrEmpty(amPm))
            {
                var dateTime = DateTime.Parse($"{hour}:{minutes} {amPm}");
                return dateTime.TimeOfDay;
            }

            return null;
        }
        private StockportGovUK.NetStandard.Models.Verint.Organisation GetOrganisationElementValue(string key, FormAnswers formAnswers)
        {
            var dateObject = new StockportGovUK.NetStandard.Models.Verint.Organisation();
            var organisationKey = $"{key}-organisation";
            var organisationDescriptionKey = $"{key}-organisation-description";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == organisationKey || _.QuestionId == organisationDescriptionKey)
                .ToList();

            dateObject.Reference = value.FirstOrDefault(_ => _.QuestionId == organisationKey)?.Response ?? string.Empty;
            dateObject.Name = value.FirstOrDefault(_ => _.QuestionId == organisationDescriptionKey)?.Response ?? string.Empty;

            return dateObject;
        }
        private int? GetNumericElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == key)
                .FirstOrDefault();

            if (value == null || string.IsNullOrEmpty(value.Response))
            {
                return null;
            }
            return int.Parse(value.Response);
        }
        private DateTime? GetDatePickerElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == key)
                .FirstOrDefault();

            if (value == null || string.IsNullOrEmpty(value.Response))
            {
                return null;
            }

            return DateTime.Parse(value.Response);
        }
        private List<string> GetCheckboxElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == key)
                .FirstOrDefault();

            if (value == null || string.IsNullOrEmpty(value.Response))
            {
                return new List<string>();
            }
            var val = value.Response.Split(",");
            return new List<string>(val);
        }
    }
}