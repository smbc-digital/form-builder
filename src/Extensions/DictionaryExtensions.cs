using System.Collections.Generic;
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
                    else if (item.Key.EndsWith(BookingConstants.APPOINTMENT_DATE) && !string.IsNullOrEmpty(item.Value[0]))
                    {
                        var bookingDetails = item.Value[0].Split('|');
                        if (!string.IsNullOrEmpty(bookingDetails[0]))
                        {
                            normalisedFormData.Add(item.Key, bookingDetails[0]);
                        }
                        if (!string.IsNullOrEmpty(bookingDetails[1]))
                        {
                            normalisedFormData.Add($"{item.Key}{BookingConstants.APPOINTMENT_FULL_DAY_START_TIME}", bookingDetails[1]);
                        }
                        if (!string.IsNullOrEmpty(bookingDetails[2]))
                        {
                            normalisedFormData.Add($"{item.Key}{BookingConstants.APPOINTMENT_FULL_DAY_END_TIME}", bookingDetails[2]);
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

            return normalisedFormData;
        }
    }
}