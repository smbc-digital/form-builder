using System.Collections.Generic;

namespace form_builder.Configuration
{
    public class PaymentInformationConfiguration
    {
        public List<PaymentInformation> PaymentInformationConfigurations { get; set; }

        public class PaymentInformation
        {
            public string formName { get; set; }
            public Settings settings { get; set; }
        }

        public class Settings
        {
            public string accountReference { get; set; }
            public string amount { get; set; }
            public string catalogueId { get; set; }
        }
    }
}