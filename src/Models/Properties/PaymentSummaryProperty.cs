using System.Collections.Generic;

namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public List<SubmitSlug> CalculationSlugs { get; set; }

        public string PaymentAmount { get; set; } = string.Empty;
    }
}