using System.Collections.Generic;

namespace form_builder.Configuration
{
    public class PaymentInformationConfiguration
    {
        public List<PaymentInformation> PaymentConfigs { get; set; }

    }

    public class PaymentInformation
    {
        public string FormName { get; set; }
        public Settings Settings { get; set; }
    }

    public class Settings
    {
        public string AccountReference { get; set; }
        public string Amount { get; set; }
        public string CatalogueId { get; set; }
    }
}