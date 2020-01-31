using form_builder.Cache;
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
        private readonly Mock<ICache> _mockCache = new Mock<ICache>();
        private readonly Mock<IOptions<DistrbutedCacheConfiguration>> _mockDistrbutedCacheSettings = new Mock<IOptions<DistrbutedCacheConfiguration>>();
        private readonly Mock<IOptions<DistrbutedCacheExpirationConfiguration>> _mockDistrbutedCacheExpirationSettings = new Mock<IOptions<DistrbutedCacheExpirationConfiguration>>();

        public PayServiceTests()
        {
            _paymentProvider.Setup(_ => _.ProviderName).Returns("testPaymentProvider");

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<ESchemaType>()))

            .ReturnsAsync(new List<PaymentInformation> {
                new PaymentInformation
                {
                    FormName = "testForm",
                    PaymentProvider = "testPaymentProvider"
                },
                new PaymentInformation
                {
                    FormName = "testFormwithnovalidpayment",
                    PaymentProvider = "invalidPaymentPorvider"
                }
            });

            _mockDistrbutedCacheSettings.Setup(_ => _.Value).Returns(new DistrbutedCacheConfiguration
            {
                UseDistrbutedCache = true
            });

            _mockDistrbutedCacheExpirationSettings.Setup(_ => _.Value).Returns(new DistrbutedCacheExpirationConfiguration
            {
                UserData = 30,
                PaymentConfiguration = 5
            });

            var paymentProviderItems = new List<IPaymentProvider> { _paymentProvider.Object };
            _mockPaymentProvider.Setup(m => m.GetEnumerator()).Returns(() => paymentProviderItems.GetEnumerator());

            _service = new PayService(_mockPaymentProvider.Object, _mockLogger.Object, _mockGateway.Object, _mockCache.Object, _mockDistrbutedCacheExpirationSettings.Object, _mockDistrbutedCacheSettings.Object);
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
        public async Task ProcessPaymentResponse_ShouldThrowApplicationException_WhenPaymentConfig_IsNull()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPaymentResponse("nonexistanceform", "12345"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowApplicationException_WhenPaymentProvider_IsNull()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPaymentResponse("nonexistanceform", "12345"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowException_WhenPaymentProviderThrows()
        {
            _paymentProvider.Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws<Exception>();

            await Assert.ThrowsAsync<Exception>(() => _service.ProcessPaymentResponse("testForm", "12345"));
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldReturnPaymentReference_OnSuccessfull_PaymentProviderCall()
        {
            _paymentProvider.Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Returns("12345");

            var result = await _service.ProcessPaymentResponse("testForm", "12345");

            Assert.IsType<string>(result);
            Assert.NotNull(result);
        }
    }
}