using form_builder.Configuration;
using form_builder.Providers.PaymentProvider;
using form_builder.Services.PayService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class PayServiceTests
    {
        private readonly PayService _service;
        private readonly Mock<ILogger<PayService>> _mockLogger = new Mock<ILogger<PayService>>();
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();
        private readonly Mock<IEnumerable<IPaymentProvider>> _mockPaymentProvider = new Mock<IEnumerable<IPaymentProvider>>();
        private readonly Mock<IPaymentProvider> _paymentProvider = new Mock<IPaymentProvider>();
        private readonly Mock<IOptions<PaymentInformationConfiguration>> _mockPaymentInformation = new Mock<IOptions<PaymentInformationConfiguration>>();

        public PayServiceTests()
        {
            _paymentProvider.Setup(_ => _.ProviderName).Returns("testPaymentProvider");

            _mockPaymentInformation.Setup(_ => _.Value).Returns(new PaymentInformationConfiguration
            {
                PaymentConfigs = new List<PaymentInformation>
               {
                new PaymentInformation
                {
                    FormName = "testForm",
                    PaymentProvider = "testPaymentProvider"
                },
                new PaymentInformation
                {
                    FormName = "testFormwithnovalidpayment",
                    PaymentProvider = "invalidPaymentPorvider"
                },
               }
            });

            var paymentProviderItems = new List<IPaymentProvider> { _paymentProvider.Object };
            _mockPaymentProvider.Setup(m => m.GetEnumerator()).Returns(() => paymentProviderItems.GetEnumerator());

            _service = new PayService(_mockPaymentProvider.Object, _mockLogger.Object, _mockGateway.Object, _mockPaymentInformation.Object);
        }


        [Fact]
        public async Task ProcessPayment_ShouldThrowApplicationException_WhenPaymentConfig_IsNull()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPayment("nonexistanceform", "page-one", "12345", "guid"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPayment_ShouldThrowApplicationException_WhenPaymentProvider_IsNull()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPayment("testFormwithnovalidpayment", "page-one", "12345", "guid"));

            Assert.Equal("PayService:: No payment provider configure for invalidPaymentPorvider", result.Message);
        }

        [Fact]
        public async Task ProcessPayment_ShouldCallPaymentProvider_And_ReturnUrl()
        {
            _paymentProvider.Setup(_ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PaymentInformation>()))
                .ReturnsAsync("url");

            var result = await _service.ProcessPayment("testForm", "page-one", "12345", "guid");

            _paymentProvider.Verify(_ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PaymentInformation>()), Times.Once);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ProcessPaymentResponse_ShouldThrowApplicationException_WhenPaymentConfig_IsNull()
        {
            var result = Assert.Throws<ApplicationException>(() => _service.ProcessPaymentResponse("nonexistanceform", "12345"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public void ProcessPaymentResponse_ShouldThrowApplicationException_WhenPaymentProvider_IsNull()
        {
            var result = Assert.Throws<ApplicationException>(() => _service.ProcessPaymentResponse("nonexistanceform", "12345"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public void ProcessPaymentResponse_ShouldThrowException_WhenPaymentProviderThrows()
        {
            _paymentProvider.Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws<Exception>();

            Assert.Throws<Exception>(() => _service.ProcessPaymentResponse("testForm", "12345"));
        }

        [Fact]
        public void ProcessPaymentResponse_ShouldReturnPaymentReference_OnSuccessfull_PaymentProviderCall()
        {
            _paymentProvider.Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Returns("12345");

            var result = _service.ProcessPaymentResponse("testForm", "12345");

            Assert.IsType<string>(result);
            Assert.NotNull(result);
        }
    }
}