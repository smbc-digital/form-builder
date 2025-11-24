using form_builder.Constants;

namespace form_builder.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToNormaliseDictionary(this Dictionary<string, string[]> formData, string subPath)
        {
            Dictionary<string, dynamic> normalisedFormData = new();
            normalisedFormData.Add(LookUpConstants.SubPathViewModelKey, subPath);

            foreach (var item in formData)
            {
                if (item.Key.EndsWith(FileUploadConstants.SUFFIX))
                    continue;

                for (int x = 0; x < item.Value.Length; x++)
                {
                    if (item.Value[x] is null) continue;

                    string lessThan = "<";
                    if (item.Value[x].Contains(lessThan))
                        item.Value[x] = item.Value[x].Replace(lessThan, "%3C");

                    string moreThan = ">";
                    if (item.Value[x].Contains(moreThan))
                        item.Value[x] = item.Value[x].Replace(moreThan, "%3E");
                }

                if (item.Value.Length.Equals(1))
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
                    normalisedFormData.Add(item.Key, string.Join(", ", item.Value));
                }
            }

            normalisedFormData = CleanUpBookingTimes(normalisedFormData);

            return normalisedFormData;
        }

        private static Dictionary<string, object> CleanUpBookingTimes(Dictionary<string, object> normalisedFormData)
        {
            if (!normalisedFormData.Any(_ => _.Key.EndsWith(BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX)))
                return normalisedFormData;

            var selectedTimeNoJsKey = $"-{BookingConstants.APPOINTMENT_FULL_TIME_OF_DAY_SUFFIX}";

            var appointmentDate = normalisedFormData.FirstOrDefault(_ => _.Key.EndsWith($"-{BookingConstants.APPOINTMENT_DATE}"));
            if (DateTime.TryParse((string)appointmentDate.Value, out var parsedDate))
            {
                var selectedTimePeriodKey = $"{parsedDate.Day}{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}";
                var selectedTimePeriod = normalisedFormData[selectedTimePeriodKey];
                var selectedTimeKey = $"-{parsedDate.Day}-{selectedTimePeriod}";

                var bookingElementId = appointmentDate.Key.Remove(appointmentDate.Key.Length - BookingConstants.APPOINTMENT_DATE.Length, BookingConstants.APPOINTMENT_DATE.Length);

                if (normalisedFormData.Any(_ => _.Key.EndsWith(selectedTimeKey)))
                {
                    var selectedTime = normalisedFormData
                        .First(_ => _.Key.EndsWith(selectedTimeKey))
                        .Value
                        .ToString().Split('|');

                    normalisedFormData = normalisedFormData
                        .Where(_ => !_.Key.EndsWith($"{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}"))
                        .Where(_ => !_.Key.EndsWith($"-{BookingConstants.APPOINTMENT_TIME_OF_DAY_MORNING}") &&
                        !_.Key.EndsWith($"-{BookingConstants.APPOINTMENT_TIME_OF_DAY_AFTERNOON}"))
                        .ToDictionary(x => x.Key, x => (dynamic)x.Value);

                    if (DateTime.TryParse(selectedTime[0], out DateTime parsedStartTime) &&
                        DateTime.TryParse(selectedTime[1], out DateTime parsedEndTime))
                    {
                        normalisedFormData.Add($"{bookingElementId}{BookingConstants.APPOINTMENT_START_TIME}", parsedStartTime.ToString());
                        normalisedFormData.Add($"{bookingElementId}{BookingConstants.APPOINTMENT_END_TIME}", parsedEndTime.ToString());
                    }
                    return normalisedFormData;
                }

                if (normalisedFormData.Any(_ => _.Key.EndsWith(selectedTimeNoJsKey)))
                {
                    var times = normalisedFormData
                        .First(_ => _.Key.EndsWith(selectedTimeNoJsKey))
                        .Value
                        .ToString().Split('|');

                    if (times.Length < 2)
                        return normalisedFormData;

                    if (!DateTime.TryParse(times[0], out var appointmentStart) ||
                        !DateTime.TryParse(times[1], out var appointmentEnd))
                        return normalisedFormData;

                    if (appointmentStart.Day > parsedDate.Day || appointmentStart.Day < parsedDate.Day)
                    {
                        normalisedFormData.Remove(appointmentDate.Key);
                        normalisedFormData.Add(appointmentDate.Key, new DateTime(appointmentStart.Year, appointmentStart.Month, appointmentStart.Day).ToString());
                    }
                    normalisedFormData.Add($"{bookingElementId}{BookingConstants.APPOINTMENT_START_TIME}", appointmentStart.ToString());
                    normalisedFormData.Add($"{bookingElementId}{BookingConstants.APPOINTMENT_END_TIME}", appointmentEnd.ToString());
                    return normalisedFormData;
                }

                normalisedFormData.Where(_ => !_.Key.EndsWith($"{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}"))
                    .Where(_ => !_.Key.EndsWith($"-{BookingConstants.APPOINTMENT_TIME_OF_DAY_MORNING}") && !_.Key.EndsWith($"-{BookingConstants.APPOINTMENT_TIME_OF_DAY_AFTERNOON}"))
                    .ToDictionary(x => x.Key, x => (dynamic)x.Value);
                normalisedFormData.Add($"{bookingElementId}{BookingConstants.APPOINTMENT_START_TIME}", string.Empty);
                return normalisedFormData;
            }

            if (normalisedFormData.Any(_ => _.Key.EndsWith(selectedTimeNoJsKey)))
            {
                var timeValue = normalisedFormData.First(_ => _.Key.EndsWith(selectedTimeNoJsKey));
                var bookingElementId = timeValue.Key.Split("-")[0];

                var times = timeValue.Value.ToString().Split('|');
                if (times.Length < 2)
                    return normalisedFormData;

                if (!DateTime.TryParse(times[0], out var appointmentStart) ||
                    !DateTime.TryParse(times[1], out var appointmentEnd))
                    return normalisedFormData;

                normalisedFormData.Add($"{bookingElementId}-{BookingConstants.APPOINTMENT_DATE}", new DateTime(appointmentStart.Year, appointmentStart.Month, appointmentStart.Day).ToString());
                normalisedFormData.Add($"{bookingElementId}-{BookingConstants.APPOINTMENT_START_TIME}", appointmentStart.ToString());
                normalisedFormData.Add($"{bookingElementId}-{BookingConstants.APPOINTMENT_END_TIME}", appointmentEnd.ToString());
                return normalisedFormData;
            }

            return normalisedFormData;
        }
    }
}
