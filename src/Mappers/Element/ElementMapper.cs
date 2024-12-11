using System.Dynamic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.FileStorage;
using form_builder.Utils.Extensions;
using form_builder.Utils.Hash;
using Newtonsoft.Json;
using Address = StockportGovUK.NetStandard.Gateways.Models.Addresses.Address;
using Booking = StockportGovUK.NetStandard.Gateways.Models.Booking.Booking;
using File = StockportGovUK.NetStandard.Gateways.Models.FileManagement.File;
using Organisation = StockportGovUK.NetStandard.Gateways.Models.Verint.Organisation;

namespace form_builder.Mappers
{
    public class ElementMapper : IElementMapper
    {
        private readonly IEnumerable<IFileStorageProvider> _fileStorageProviders;
        private readonly IHashUtil _hashUtil;
        private readonly IConfiguration _configuration;

        public ElementMapper(IEnumerable<IFileStorageProvider> fileStorageProviders,
            IHashUtil hashUtil,
            IConfiguration configuration)
        {
            _fileStorageProviders = fileStorageProviders;
            _hashUtil = hashUtil;
            _configuration = configuration;
        }

        public async Task<T> GetAnswerValue<T>(IElement element, FormAnswers formAnswers)
        {
            var value = await GetAnswerValue(element, formAnswers);
            return (T)value;
        }

        public async Task<object> GetAnswerValue(IElement element, FormAnswers formAnswers)
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
                case EElementType.MultipleFileUpload:
                    return await GetFileUploadElementValue(key, formAnswers);
                case EElementType.Booking:
                    return GetBookingElementValue(key, formAnswers);
                default:
                    if (element.Properties.Numeric)
                        return GetNumericElementValue(key, formAnswers);

                    if (element.Properties.Decimal)
                        return GetDecimalElementValue(key, formAnswers);

                    if (element.Properties.Decimal)
                        return GetDecimalElementValue(key, formAnswers);

                    var value = formAnswers.Pages?.SelectMany(_ => _.Answers)
                       .Where(_ => _.QuestionId.Equals(key))
                       .ToList()
                       .FirstOrDefault();

                    return value?.Response ?? "";
            }
        }

        private object GetDeclarationElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages?
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value is null || string.IsNullOrEmpty(value.Response))
                return "Unchecked";

            return "Checked";
        }

        public async Task<string> GetAnswerStringValue(IElement question, FormAnswers formAnswers)
        {
            if (question.Type.Equals(EElementType.FileUpload) || question.Type.Equals(EElementType.MultipleFileUpload))
            {
                var fileInput = formAnswers.Pages?
                    .ToList()
                    .SelectMany(_ => _.Answers)
                    .ToList()
                    .FirstOrDefault(_ => _.QuestionId.Equals($"{question.Properties.QuestionId}{FileUploadConstants.SUFFIX}"))?.Response;

                if (fileInput is null)
                    return string.Empty;

                List<FileUploadModel> fileUploadData = JsonConvert.DeserializeObject<List<FileUploadModel>>(fileInput.ToString());

                if (question.Type.Equals(EElementType.FileUpload))
                    return fileUploadData.FirstOrDefault()?.TrustedOriginalFileName;

                return fileUploadData.Any() ? fileUploadData.Select(_ => _.TrustedOriginalFileName).Aggregate((cur, acc) => $"{acc} \\r\\n\\ {cur}") : string.Empty;
            }

            object value = await GetAnswerValue(question, formAnswers);

            if (value is null)
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
                    var selectValue = question.Properties.Options.FirstOrDefault(_ => _.Value.Equals(value.ToString()));
                    return selectValue?.Text ?? string.Empty;
                case EElementType.Checkbox:
                    var answerCheckbox = string.Empty;
                    var list = (List<string>)value;
                    list.ForEach((answersCheckbox) => answerCheckbox += $" {question.Properties.Options.FirstOrDefault(_ => _.Value.Equals(answersCheckbox))?.Text ?? string.Empty},");
                    return answerCheckbox.EndsWith(",") ? answerCheckbox.Remove(answerCheckbox.Length - 1).Trim() : answerCheckbox.Trim();
                case EElementType.Organisation:
                    var orgValue = (Organisation)value;
                    return !string.IsNullOrEmpty(orgValue.Name) ? orgValue.Name : string.Empty;
                case EElementType.Address:
                    var addressValue = (Address)value;
                    if (!string.IsNullOrEmpty(addressValue.SelectedAddress))
                        return addressValue.SelectedAddress;
                    var manualLine2Text = string.IsNullOrWhiteSpace(addressValue.AddressLine2) ? string.Empty : $",{addressValue.AddressLine2}";
                    return string.IsNullOrWhiteSpace(addressValue.AddressLine1) ? string.Empty : $"{addressValue.AddressLine1}{manualLine2Text},{addressValue.Town},{addressValue.Postcode}";
                case EElementType.Booking:
                    var bookingValue = (Booking)value;
                    return bookingValue.Date.Equals(DateTime.MinValue) && bookingValue.StartTime.Equals(DateTime.MinValue) ? string.Empty : $"{bookingValue.Date.ToFullDateFormat()} at {bookingValue.StartTime.ToTimeFormat()} to {bookingValue.EndTime.ToTimeFormat()}";
                case EElementType.Street:
                    var streetValue = (Address)value;
                    return string.IsNullOrEmpty(streetValue.PlaceRef) && string.IsNullOrEmpty(streetValue.SelectedAddress) ? string.Empty : streetValue.SelectedAddress;

                default:
                    return value.ToString();
            }
        }

        private async Task<object> GetFileUploadElementValue(string key, FormAnswers formAnswers)
        {
            key = $"{key}{FileUploadConstants.SUFFIX}";

            List<File> listOfFiles = new();

            var value = formAnswers.Pages?
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value is null || value.Response is null)
                return null;

            List<FileUploadModel> uploadedFiles = JsonConvert.DeserializeObject<List<FileUploadModel>>(value.Response.ToString());

            var fileStorageProvider = _fileStorageProviders.Get(_configuration["FileStorageProvider:Type"]);

            foreach (var file in uploadedFiles)
            {
                var fileData = await fileStorageProvider.GetString(file.Key);

                if (fileData is null)
                    throw new Exception($"ElementMapper::GetFileUploadElementValue: An error has occurred while attempting to retrieve an uploaded file with key: {file.Key} from the distributed cache");

                File model = new();
                model.Content = fileData;
                model.TrustedOriginalFileName = file.TrustedOriginalFileName.ToMaxSpecifiedStringLengthForFileName(100);
                model.UntrustedOriginalFileName = file.UntrustedOriginalFileName.ToMaxSpecifiedStringLengthForFileName(100);
                model.KeyName = key;

                listOfFiles.Add(model);
            }

            return listOfFiles;

        }


        private Booking? GetBookingElementValue(string key, FormAnswers formAnswers)
        {
            var bookingObject = new Booking();

            var appointmentId = $"{key}-{BookingConstants.RESERVED_BOOKING_ID}";
            var appointmentDate = $"{key}-{BookingConstants.RESERVED_BOOKING_DATE}";
            var appointmentStartTime = $"{key}-{BookingConstants.RESERVED_BOOKING_START_TIME}";
            var appointmentEndTime = $"{key}-{BookingConstants.RESERVED_BOOKING_END_TIME}";
            var appointmentLocation = $"{key}-{BookingConstants.APPOINTMENT_LOCATION}";

            var value = formAnswers.Pages?.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId.Equals(appointmentId) || _.QuestionId.Equals(appointmentDate) ||
                            _.QuestionId.Equals(appointmentStartTime) || _.QuestionId.Equals(appointmentEndTime) ||
                            _.QuestionId.Equals(appointmentLocation))
                .ToList();

            if (value is null || !value.Any())
                return null;

            var bookingId = value.FirstOrDefault(_ => _.QuestionId.Equals(appointmentId))?.Response;
            var bookingDate = value.FirstOrDefault(_ => _.QuestionId.Equals(appointmentDate))?.Response;
            var bookingStartTime = value.FirstOrDefault(_ => _.QuestionId.Equals(appointmentStartTime))?.Response;
            var bookingEndTime = value.FirstOrDefault(_ => _.QuestionId.Equals(appointmentEndTime))?.Response;
            var bookingLocation = value.FirstOrDefault(_ => _.QuestionId.Equals(appointmentLocation))?.Response;
            bookingObject.Id = bookingId is not null ? Guid.Parse(bookingId) : Guid.Empty;
            bookingObject.HashedId = bookingId is not null ? _hashUtil.Hash(bookingObject.Id.ToString()) : string.Empty;
            bookingObject.Date = bookingDate is not null ? DateTime.Parse(bookingDate) : DateTime.MinValue;
            bookingObject.StartTime = bookingStartTime is not null ? DateTime.Parse(bookingStartTime) : DateTime.MinValue;
            bookingObject.EndTime = bookingEndTime is not null ? DateTime.Parse(bookingEndTime) : DateTime.MinValue;
            bookingObject.Location = bookingLocation;

            return bookingObject.IsEmpty() ? null : bookingObject;
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

            var value = formAnswers.Pages?.SelectMany(_ => _.Answers)
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

            var value = formAnswers.Pages?.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId.Equals(usrnKey) || _.QuestionId.Equals(streetDescription))
                .ToList();

            addressObject.PlaceRef = value.FirstOrDefault(_ => _.QuestionId.Equals(usrnKey))?.Response ?? string.Empty;
            addressObject.SelectedAddress = value.FirstOrDefault(_ => _.QuestionId.Equals(streetDescription))?.Response ?? null;

            return addressObject.IsEmpty() ? null : addressObject;
        }
        private DateTime? GetDateInputElementValue(string key, FormAnswers formAnswers)
        {
            var dateDayKey = $"{key}-day";
            var dateMonthKey = $"{key}-month";
            var dateYearKey = $"{key}-year";

            var value = formAnswers.Pages.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId.Equals(dateDayKey) ||
                            _.QuestionId.Equals(dateMonthKey) ||
                            _.QuestionId.Equals(dateYearKey))
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

            var value = formAnswers.Pages?.SelectMany(_ => _.Answers)
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

            var value = formAnswers.Pages?.SelectMany(_ => _.Answers)
                .Where(_ => _.QuestionId.Equals(organisationKey) || _.QuestionId.Equals(organisationDescriptionKey))
                .ToList();

            orgObject.Reference = value.FirstOrDefault(_ => _.QuestionId.Equals(organisationKey))?.Response ?? string.Empty;
            orgObject.Name = value.FirstOrDefault(_ => _.QuestionId.Equals(organisationDescriptionKey))?.Response ?? string.Empty;

            return orgObject;
        }
        private int? GetNumericElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages?
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value is null || string.IsNullOrEmpty(value.Response))
                return null;

            return int.Parse(value.Response, SystemConstants.NUMERIC_NUMBER_STYLES, null);
        }

        private decimal? GetDecimalElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages?
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value is null || string.IsNullOrEmpty(value.Response))
                return null;

            return decimal.Parse(value.Response, SystemConstants.DECIMAL_NUMBER_STYLES, null);
        }

        private DateTime? GetDatePickerElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages?
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value is null || string.IsNullOrEmpty(value.Response))
                return null;

            return DateTime.Parse(value.Response);
        }
        private List<string> GetCheckboxElementValue(string key, FormAnswers formAnswers)
        {
            var value = formAnswers.Pages?
                .SelectMany(_ => _.Answers)
                .FirstOrDefault(_ => _.QuestionId.Equals(key));

            if (value is null || string.IsNullOrEmpty(value.Response))
                return new List<string>();

            var val = value.Response.Split(",");

            return new List<string>(val);
        }
    }
}
