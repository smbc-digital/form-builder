namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public int MaxCombinedFileSize { get; set; }

        public string PageSubmitButtonLabel { get; set; }

        public bool DisplayReCaptcha { get; set; } = false;
    }
}
