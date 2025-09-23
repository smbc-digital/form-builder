using System.Reflection;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;

namespace form_builder.Builders
{
    public class ElementBuilder
    {
        private EElementType _type = EElementType.H1;
        private string _lookup = string.Empty;

        private BaseProperty _property = new();

        public Element Build()
        {
            var elementType = typeof(IElement).GetTypeInfo().Assembly
                .GetTypes()
                .FirstOrDefault(type => type.Name.Equals(_type.ToString()));

            var element = (Element)Activator.CreateInstance(elementType);

            element.Properties = _property;
            element.Lookup = _lookup;

            return element;
        }

        public ElementBuilder WithType(EElementType type)
        {
            _type = type;

            return this;
        }

        public ElementBuilder WithLookup(string lookup)
        {
            _lookup = lookup;

            return this;
        }

        public ElementBuilder WithIAG(string value)
        {
            _property.IAG = value;

            return this;
        }

        public ElementBuilder WithLookupSource(LookupSource lookupSource)
        {
            _property.LookupSources ??= new List<LookupSource>();

            _property.LookupSources.Add(lookupSource);

            return this;
        }

        public ElementBuilder WithPropertyText(string propertyText)
        {
            _property.Text = propertyText;

            return this;
        }

        public ElementBuilder WithQuestionId(string questionId)
        {
            _property.QuestionId = questionId;

            return this;
        }

        public ElementBuilder WithTargetMapping(string targetMapping)
        {
            _property.TargetMapping = targetMapping;

            return this;
        }

        public ElementBuilder WithLabel(string label)
        {
            _property.Label = label;

            return this;
        }

        public ElementBuilder WithAddressLabel(string label)
        {
            _property.AddressLabel = label;

            return this;
        }

        public ElementBuilder WithValue(string value)
        {
            _property.Value = value;

            return this;
        }

        public ElementBuilder WithListItems(List<string> listItems)
        {
            _property.ListItems = listItems;

            return this;
        }

        public ElementBuilder WithSource(string source)
        {
            _property.Source = source;

            return this;
        }

        public ElementBuilder WithAltText(string alt)
        {
            _property.AltText = alt;

            return this;
        }

        public ElementBuilder WithMaxLength(int maxLength)
        {
            _property.MaxLength = maxLength;

            return this;
        }

        public ElementBuilder WithMinLength(int minLength)
        {
            _property.MinLength = minLength;

            return this;
        }

        public ElementBuilder WithOptions(List<Option> options)
        {
            _property.Options = options;

            return this;
        }

        public ElementBuilder WithRestrictFutureDate(bool value, string validMessage = "")
        {
            _property.RestrictFutureDate = value;
            _property.ValidationMessageRestrictFutureDate = validMessage;

            return this;
        }

        public ElementBuilder WithRestrictPastDate(bool value, string validMessage = "")
        {
            _property.RestrictPastDate = value;
            _property.ValidationMessageRestrictPastDate = validMessage;

            return this;
        }

        public ElementBuilder WithRestrictCurrentDate(bool value, string validMessage = "")
        {
            _property.RestrictCurrentDate = value;
            _property.ValidationMessageRestrictCurrentDate = validMessage;

            return this;
        }

        public ElementBuilder WithEmail(bool value)
        {
            _property.Email = value;

            return this;
        }

        public ElementBuilder WithOptional(bool value)
        {
            _property.Optional = value;

            return this;
        }

        public ElementBuilder WithAllowSingleOption(bool value)
        {
            _property.AllowSingleOption = value;

            return this;
        }

        public ElementBuilder WithDayValue(string value)
        {
            _property.Day = value;

            return this;
        }

        public ElementBuilder WithMonthValue(string value)
        {
            _property.Month = value;

            return this;
        }

        public ElementBuilder WithYearValue(string value)
        {
            _property.Year = value;
            return this;
        }

        public ElementBuilder WithAddressProvider(string value)
        {
            _property.AddressProvider = value;

            return this;
        }

        public ElementBuilder WithStreetProvider(string value)
        {
            _property.StreetProvider = value;

            return this;
        }

        public ElementBuilder WithOrganisationProvider(string value)
        {
            _property.OrganisationProvider = value;

            return this;
        }

        public ElementBuilder WithBookingProvider(string value)
        {
            _property.BookingProvider = value;

            return this;
        }

        public ElementBuilder WithAppointmentType(AppointmentType value)
        {
            if (_property.AppointmentTypes is null)
                _property.AppointmentTypes = new List<AppointmentType>();

            _property.AppointmentTypes.Add(value);

            return this;
        }

        public ElementBuilder WithLimitNextAvailableByDays(int value)
        {
            _property.LimitNextAvailableByDays = value;

            return this;
        }

        public ElementBuilder WithLimitNextAvailableFromDate(string value)
        {
            _property.LimitNextAvailableFromDate = value;

            return this;
        }

        public ElementBuilder WithNumeric(bool value)
        {
            _property.Numeric = value;

            return this;
        }

        public ElementBuilder WithHint(string value)
        {
            _property.Hint = value;

            return this;
        }

        public ElementBuilder WithRegex(string value)
        {
            _property.Regex = value;

            return this;
        }

        public ElementBuilder WithMax(string value)
        {
            _property.Max = value;

            return this;
        }

        public ElementBuilder WithMin(string value)
        {
            _property.Min = value;

            return this;
        }

        public ElementBuilder WithButtonId(string buttonId)
        {
            _property.ButtonId = buttonId;

            return this;
        }

        public ElementBuilder WithButtonName(string buttonName)
        {
            _property.ButtonName = buttonName;

            return this;
        }

        public ElementBuilder WithUpperLimitValidationMessage(string message)
        {
            _property.UpperLimitValidationMessage = message;

            return this;
        }

        public ElementBuilder WithAcceptedMimeType(string type)
        {
            if (_property.AllowedFileTypes is null)
                _property.AllowedFileTypes = new List<string>();

            _property.AllowedFileTypes.Add(type);

            return this;
        }

        public ElementBuilder WithDocumentType(EDocumentType docType)
        {
            _property.DocumentType = docType;

            return this;
        }

        public ElementBuilder WithStockportPostcode(bool isStockportPostCode)
        {
            _property.StockportPostcode = isStockportPostCode;

            return this;
        }

        public ElementBuilder WithFullUKPostcode(bool isFullUKPostCode)
        {
            _property.FullUKPostcode = isFullUKPostCode;

            return this;
        }

        public ElementBuilder WithDisableManualAddress(bool disableManualAddress)
        {
            _property.DisableManualAddress = disableManualAddress;

            return this;
        }

        public ElementBuilder WithNoManualAddressDetailText(string noManualAddressDetailText)
        {
            _property.NoManualAddressDetailText = noManualAddressDetailText;

            return this;
        }

        public ElementBuilder WithDisableOnClick(bool value)
        {
            _property.DisableOnClick = value;

            return this;
        }

        public ElementBuilder WithCustomValidationMessage(string value)
        {
            _property.CustomValidationMessage = value;

            return this;
        }

        public ElementBuilder WithMaxCombinedFileSize(int value)
        {
            _property.MaxCombinedFileSize = value;

            return this;
        }

        public ElementBuilder WithMaxFileSize(int value)
        {
            _property.MaxFileSize = value;

            return this;
        }

        public ElementBuilder WithFileUploadQuestionIds(List<string> value)
        {
            _property.FileUploadQuestionIds = value;

            return this;
        }

        public ElementBuilder WithCheckYourBooking(bool value)
        {
            _property.CheckYourBooking = value;

            return this;
        }

        public ElementBuilder WithClassName(string value)
        {
            _property.ClassName = value;

            return this;
        }

        public ElementBuilder WithOpenInTab(bool value)
        {
            _property.OpenInTab = value;

            return this;
        }

        public ElementBuilder WithUrl(string value)
        {
            _property.Url = value;

            return this;
        }

        public ElementBuilder WithConditionalElement(bool value)
        {
            _property.isConditionalElement = value;

            return this;
        }

        public ElementBuilder WithIsDateBeforeAbsolute(string value)
        {
            _property.IsDateBeforeAbsolute = value;
            return this;
        }

        public ElementBuilder WithIsDateBefore(string value)
        {
            _property.IsDateBefore = value;
            return this;
        }


        public ElementBuilder WithIsDateAfter(string value)
        {
            _property.IsDateAfter = value;
            return this;
        }

        public ElementBuilder WithIsDateAfterAbsolute(string value)
        {
            _property.IsDateAfterAbsolute = value;
            return this;
        }

        public ElementBuilder WithNoAvailableTimeForBookingType(string value)
        {
            _property.NoAvailableTimeForBookingType = value;
            return this;
        }

        public ElementBuilder WithCustomerAddressId(string value)
        {
            _property.CustomerAddressId = value;

            return this;
        }

        public ElementBuilder WithIsDateEqualityAllowed(bool allowed)
        {
            _property.IsDateEqualityAllowed = allowed;

            return this;
        }

        public ElementBuilder WithSummaryLabel(string value)
        {
            _property.SummaryLabel = value;

            return this;
        }

        public ElementBuilder WithOpeningTagValue(bool value)
        {
            _property.OpeningTag = value;

            return this;
        }

        public ElementBuilder WithOrderOptionsAlphabetically(bool value)
        {
            _property.OrderOptionsAlphabetically = value;

            return this;
        }

        public ElementBuilder withSummarySection(Section value)
        {
            if (_property.Sections is null)
                _property.Sections = new List<Section>();

            _property.Sections.Add(value);

            return this;
        }

        public ElementBuilder WithLabelAsH1(bool value)
        {
            _property.LabelAsH1 = value;

            return this;
        }

        public ElementBuilder WithNestedElement(Element element)
        {
            if (_property.Elements is null)
                _property.Elements = new List<IElement> { element };
            else
                _property.Elements.Add(element);

            return this;
        }

        public ElementBuilder WithMaximumFieldsets(int value)
        {
            _property.MaximumFieldsets = value;

            return this;
        }

        public ElementBuilder WithMinimumFieldsets(int value)
        {
            _property.MinimumFieldsets = value;

            return this;
        }

        public ElementBuilder WithFirstLabel(string value)
        {
            _property.FirstLabel = value;

            return this;
        }

        public ElementBuilder WithIsConditionalElement(bool value)
        {
            _property.isConditionalElement = value;

            return this;
        }

        public ElementBuilder WithAppendText(string value)
        {
            _property.AppendText = value;
            return this;
        }

        public ElementBuilder WithAutoConfirm(bool value)
        {
            _property.AutoConfirm = value;

            return this;
        }

        public ElementBuilder WithExclusiveCheckboxValidationMessage(string value)
        {
            _property.ExclusiveCheckboxValidationMessage = value;

            return this;
        }


        public ElementBuilder WithSelectExactly(int value)
        {
            _property.SelectExactly = value;
            return this;
        }

        public ElementBuilder WithOutsideRange(string value)
        {
            _property.OutsideRange = value;

            return this;
        }

        public ElementBuilder WithWithinRange(string value)
        {
            _property.WithinRange = value;

            return this;
        }

        public ElementBuilder WithIsFutureDateAfterRelative(string value, string validMessage = "")
        {
            _property.IsFutureDateAfterRelative = value;
            _property.ValidationMessageIsFutureDateAfterRelative = validMessage;

            return this;
        }

        public ElementBuilder WithIsFutureDateBeforeRelative(string value, string validMessage = "")
        {
            _property.IsFutureDateBeforeRelative = value;
            _property.ValidationMessageIsFutureDateBeforeRelative = validMessage;

            return this;
        }

        public ElementBuilder WithIsPastDateBeforeRelative(string value, string validMessage = "")
        {
            _property.IsPastDateBeforeRelative = value;
            _property.ValidationMessageIsPastDateBeforeRelative = validMessage;

            return this;
        }

        public ElementBuilder WithIsPastDateAfterRelative(string value, string validMessage = "")
        {
            _property.IsPastDateAfterRelative = value;
            _property.ValidationMessageIsPastDateAfterRelative = validMessage;

            return this;
        }

        public ElementBuilder WithDecimal(bool value)
        {
            _property.Decimal = value;

            return this;
        }

        public ElementBuilder WithDecimalValidationMessage(string value)
        {
            _property.DecimalValidationMessage = value;

            return this;
        }

        public ElementBuilder WithNotAnIntegerValidationMessage(string value)
        {
            _property.NotAnIntegerValidationMessage = value;

            return this;
        }

        public ElementBuilder WithDecimalPlaces(int value)
        {
            _property.DecimalPlaces = value;

            return this;
        }

        public ElementBuilder WithDecimalPlacesValidationMessage(string value)
        {
            _property.DecimalPlacesValidationMessage = value;

            return this;
        }

        public ElementBuilder WithWarning(string value)
        {
            _property.Warning = value;

            return this;
        }

        public ElementBuilder WithOptionalIf(string questionId, string comparisonValue, ECondition conditionType)
        {
            _property.OptionalIf.QuestionId = questionId;
            _property.OptionalIf.ComparisonValue = comparisonValue;
            _property.OptionalIf.ConditionType = conditionType;

            return this;
        }

        public ElementBuilder WithValidatePostcode(bool validate)
        {
            _property.ValidatePostcode = validate;

            return this;
        }

        public ElementBuilder WithSetAutofocus(bool autofocus)
        {
            _property.SetAutofocus= autofocus;

            return this;
        }

        public ElementBuilder WithStreetMissingText(string streetMissingText)
        {
            _property.StreetMissingText = streetMissingText;

            return this;
        }
    }
}