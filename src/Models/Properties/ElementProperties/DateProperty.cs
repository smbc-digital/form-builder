namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public bool RestrictFutureDate { get; set; } = false;

        public bool RestrictPastDate { get; set; } = false;

        public bool RestrictCurrentDate { get; set; } = false;

        public string OutsideRange { get; set; }
        public string OutsideRangeType => string.IsNullOrEmpty(OutsideRange) ? string.Empty : OutsideRange.Substring(OutsideRange.LastIndexOf('-') + 1).Trim().ToUpper();
        public string WithinRange { get; set; }
        public string WithinRangeType => string.IsNullOrEmpty(WithinRange) ? string.Empty : WithinRange.Substring(WithinRange.LastIndexOf('-') + 1).Trim().ToUpper();
        public string IsFutureDateAfterRelative { get; set; } //"2-d"
        public string IsFutureDateAfterRelativeType => string.IsNullOrEmpty(IsFutureDateAfterRelative) ? string.Empty : IsFutureDateAfterRelative.Substring(IsFutureDateAfterRelative.LastIndexOf('-') + 1).Trim().ToUpper();  //D
        public string Day { get; set; } = string.Empty;

        public string Month { get; set; } = string.Empty;

        public string Year { get; set; } = string.Empty;

        public string ValidationMessageInvalidDate { get; set; } = string.Empty;

        public string ValidationMessageRestrictFutureDate { get; set; } = string.Empty;

        public string ValidationMessageRestrictPastDate { get; set; } = string.Empty;

        public string ValidationMessageRestrictCurrentDate { get; set; } = string.Empty;

        public string ValidationMessageIsFutureDateAfterRelative { get; set; } = string.Empty;
    }
}