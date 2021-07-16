using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Configuration
{
    public class PaymentInformation
    {
        public string FormName { get; set; }
        public string PaymentProvider { get; set; }
        public Settings Settings { get; set; }

        public bool IsServicePay() => !string.IsNullOrEmpty(Settings.ServicePayReference);
    }

    public class Settings
    {
        public string AccountReference { get; set; }
        public string ServicePayReference { get; set; }
        public string ServicePayNarrative { get; set; }
        public string Amount { get; set; }
        public string CatalogueId { get; set; }
        public string Description { get; set; }
        public SubmitSlug CalculationSlug { get; set; }
    }
}