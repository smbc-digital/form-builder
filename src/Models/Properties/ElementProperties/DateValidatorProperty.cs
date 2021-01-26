namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string IsDateBefore { get; set; } = string.Empty;
                
        public string IsDateBeforeAbsolute { get; set; } = string.Empty;

        public string IsDateBeforeValidationMessage { get; set; } = string.Empty;

        public string IsDateAfter { get; set; } = string.Empty;
        
        public string IsDateAfterAbsolute { get; set; }  = string.Empty;

        public string IsDateAfterValidationMessage { get; set; } = string.Empty;      
    }
}