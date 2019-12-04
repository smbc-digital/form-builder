using System.Collections.Generic;

namespace form_builder.Models
{
    public class Property
    {
        public string Text { get; set; }
        public string QuestionId { get; set; }
        public string Label { get; set; }
        public bool Optional { get; set; } = false;
        public bool? Email { get; set; }
        public bool? Postcode { get; set; }
        public bool? StockportPostcode { get; set; }
        public bool Numeric { get; set; } = false;
        public List<Option> Options { get; set; }
        public string ButtonId { get; set; }
        public string MaxLength { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
        public string SelectHint { get; set; } = string.Empty;
        public string CustomValidationMessage { get; set; } = string.Empty;
        public string ValidationMessageRestrictFutureDate { get; set; } = string.Empty;
        public string ValidationMessageRestrictPastDate { get; set; } = string.Empty;
        public string ValidationMessageRestrictCurrentDate { get; set; } = string.Empty;
        public string ValidationMessageInvalidDate { get; set; } = string.Empty;
        public string SelectCustomValidationMessage { get; set; } = string.Empty;
        public string ClassName { get; set; }
        public List<string> ListItems = new List<string>();
        public string Source { get; set; }
        public string AltText { get; set; }
        public bool Checked { get; set; }
        public string AddressProvider { get; set; }
        public string PostcodeLabel { get; set; } = string.Empty;
        public string AddressLabel { get; set; } = string.Empty;
        public string AddressManualAddressLine1 { get; set; } = string.Empty;
        public string AddressManualAddressLine2 { get; set; } = string.Empty;
        public string AddressManualAddressTown { get; set; } = string.Empty;
        public string AddressManualAddressPostcode { get; set; } = string.Empty;

        public string AddressManualHint { get; set; } = string.Empty;

        public string StreetProvider { get; set; }
        public string SelectLabel { get; set; } = string.Empty;
        public string StreetLabel { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public bool RestrictFutureDate { get; set; } = false;
        public bool RestrictPastDate { get; set; } = false;
        public bool RestrictCurrentDate { get; set; } = false;
    }
}