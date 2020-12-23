using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Constants;

namespace form_builder.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string,object> ToNormaliseDictionary(this Dictionary<string, string[]> formData, string subPath)
        {
            var normalisedFormData = new Dictionary<string, dynamic>();
            normalisedFormData.Add(LookUpConstants.SubPathViewModelKey, subPath);
            
            foreach (var item in formData)
            {
                if (item.Key.EndsWith(FileUploadConstants.SUFFIX))
                    continue;

                if (item.Value.Length == 1)
                {
                    if (item.Key.EndsWith(AddressConstants.SELECT_SUFFIX) && !string.IsNullOrEmpty(item.Value[0]))
                    {
                        var addressDetails = item.Value[0].Split('|');
                        if (!string.IsNullOrEmpty(addressDetails[0]))
                        {
                            normalisedFormData.Add($"{item.Key}", addressDetails[0]);
                        }
                        if (!string.IsNullOrEmpty(addressDetails[1]))
                        {
                            normalisedFormData.Add($"{item.Key}-description", addressDetails[1]);
                        }
                    }
                    else if (item.Key.EndsWith(StreetConstants.SELECT_SUFFIX) && !string.IsNullOrEmpty(item.Value[0]))
                    {
                        var streetDetails = item.Value[0].Split('|');
                        if (!string.IsNullOrEmpty(streetDetails[0]))
                        {
                            normalisedFormData.Add($"{item.Key}", streetDetails[0]);
                        }
                        if (!string.IsNullOrEmpty(streetDetails[1]))
                        {
                            normalisedFormData.Add($"{item.Key}-description", streetDetails[1]);
                        }
                    }
                    else if (item.Key.EndsWith(OrganisationConstants.SELECT_SUFFIX) && !string.IsNullOrEmpty(item.Value[0]))
                    {
                        var organisationDetails = item.Value[0].Split('|');
                        if (!string.IsNullOrEmpty(organisationDetails[0]))
                        {
                            normalisedFormData.Add($"{item.Key}", organisationDetails[0]);
                        }
                        if (!string.IsNullOrEmpty(organisationDetails[1]))
                        {
                            normalisedFormData.Add($"{item.Key}-description", organisationDetails[1]);
                        }
                    }
                    else
                    {
                        normalisedFormData.Add(item.Key, item.Value[0]);
                    }
                }
                else
                {
                    normalisedFormData.Add(item.Key, string.Join(",", item.Value));
                }
            }

            normalisedFormData = CleanUpBookingTimes(normalisedFormData);

            return normalisedFormData;
        }

        private static Dictionary<string,object> CleanUpBookingTimes(Dictionary<string,object> normalisedFormData)
        {
            if(!normalisedFormData.Any(_ => _.Key.EndsWith(BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX)))
                return normalisedFormData;

            var dateValue = normalisedFormData.FirstOrDefault(_ => _.Key.EndsWith($"-{BookingConstants.APPOINTMENT_DATE}"));

            if(dateValue.Value == null)
                return normalisedFormData;

            DateTime.TryParse((string)dateValue.Value, out DateTime parsedDate);

            var selectedTimePeriodKey = $"{parsedDate.Day}{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}";
            var selectedTimePeriod = normalisedFormData[selectedTimePeriodKey];
            var selectedTimeKey = $"{dateValue.Key}-{parsedDate.Day}-{selectedTimePeriod}";

            if(!normalisedFormData.ContainsKey(selectedTimeKey))
            {
                //remove keys
                //set time as null
            }

            var selectedTime = (string) normalisedFormData[selectedTimeKey];

            var test = normalisedFormData.Where(_ => !_.Key.EndsWith($"{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}"))
                .Where(_ => !_.Key.EndsWith("-Afternoon") && !_.Key.EndsWith("-Morning"))
                .ToDictionary(x => x.Key, x => (dynamic)x.Value);
            
            test.Add($"{dateValue.Key}-{BookingConstants.APPOINTMENT_START_TIME}", DateTime.Today.Add(TimeSpan.Parse(selectedTime)));

            return normalisedFormData;
        }
    }
}