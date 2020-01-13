namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public string Date { get; set; } = string.Empty;
        public bool RestrictFutureDate { get; set; } = false;
        public bool RestrictPastDate { get; set; } = false;
        public bool RestrictCurrentDate { get; set; } = false;
        public string Day { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string ValidationMessageInvalidDate { get; set; } = string.Empty;
        public string ValidationMessageRestrictFutureDate { get; set; } = string.Empty;
        public string ValidationMessageRestrictPastDate { get; set; } = string.Empty;
        public string ValidationMessageRestrictCurrentDate { get; set; } = string.Empty;
    }
}
