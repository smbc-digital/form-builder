using form_builder.Configuration;
using Xunit;

namespace form_builder_tests.UnitTests.Configuration
{
    public class PaymentInformationConfigurationTests
    { 
        [Fact]
        public void IsServicePay_ShouldReturnTrue_IfServicePayReferenceIsSet()
        {
            var paymentInformation = new PaymentInformation
            {
                Settings = new Settings
                {
                    ServicePayReference = "test"
                }
            };

            var result = paymentInformation.IsServicePay();

            Assert.True(result);
        }

        [Fact]
        public void IsServicePay_ShouldReturnFalse_IfServicePayReferenceIsSet()
        {
            var paymentInformation = new PaymentInformation
            {
                Settings = new Settings()
            };

            var result = paymentInformation.IsServicePay();

            Assert.False(result);
        }
    }
}
