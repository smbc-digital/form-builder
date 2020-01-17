using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using System;
using System.Dynamic;
using System.Linq;
using Address = StockportGovUK.NetStandard.Models.Addresses.Address;

namespace form_builder.Mappers
{
    public static class ElementMapper
    {
        public static object GetAnswerValue(IElement element, FormAnswers formAnswers)
        {
            var key = element.Properties.QuestionId;

            switch (element.Type)
            {
                case EElementType.DateInput:
                    return GetDateElementValue(key, formAnswers);
                case EElementType.TimeInput:
                    return GetTimeElementValue(key, formAnswers);
                case EElementType.Address:
                    return GetAddressElementValue(key, formAnswers);
                case EElementType.Street:
                    return GetStreetElementValue(key, formAnswers);
                case EElementType.Organisation:
                    return GetOrganisationElementValue(key, formAnswers);
                default:
                    var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                       .Where(_ => _.QuestionId == key)
                       .ToList()
                       .FirstOrDefault();

                    return value?.Response ?? "";
            }
        }
        private static Address GetAddressElementValue(string key, FormAnswers formAnswers)
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
        private static Address GetStreetElementValue(string key, FormAnswers formAnswers)
        {
            var addressObject = new Address();

            var uspnKey = $"{key}-streetaddress";
            var streetDescription= $"{key}-streetaddress-description";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == uspnKey || _.QuestionId == streetDescription)
                .ToList();

            addressObject.PlaceRef = value.FirstOrDefault(_ => _.QuestionId == uspnKey)?.Response ?? string.Empty;
            addressObject.SelectedAddress = value.FirstOrDefault(_ => _.QuestionId == streetDescription)?.Response ?? string.Empty;

            return addressObject;
        }
        private static DateTime GetDateElementValue(string key, FormAnswers formAnswers)
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

            return new DateTime();
        }
        private static TimeSpan GetTimeElementValue(string key, FormAnswers formAnswers)
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

            return new TimeSpan();
        }
        private static string GetOrganisationElementValue(string key, FormAnswers formAnswers)
        {
            dynamic dateObject = new ExpandoObject();
            var organisationKey = $"{key}-organisation";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId == organisationKey)
                .ToList()
                .FirstOrDefault();

            if (value != null && !string.IsNullOrEmpty(value.Response))
                return value.Response;

            return string.Empty;
        }
    }
}