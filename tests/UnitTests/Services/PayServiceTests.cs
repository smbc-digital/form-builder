using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Providers.PaymentProvider;
using form_builder.Services.PayService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using form_builder.Services.MappingService.Entities;
using form_builder.Models;

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
        private readonly Mock<IOptions<DistrbutedCacheExpirationConfiguration>> _mockDistrbutedCacheExpirationSettings = new Mock<IOptions<DistrbutedCacheExpirationConfiguration>>();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IMappingService> _mockMappingService = new Mock<IMappingService>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnvironment = new Mock<IHostingEnvironment>();

        public PayServiceTests()
        {
            _paymentProvider.Setup(_ => _.ProviderName).Returns("testPaymentProvider");

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))

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

            _mockDistrbutedCacheExpirationSettings.Setup(_ => _.Value).Returns(new DistrbutedCacheExpirationConfiguration
            {
                UserData = 30,
                PaymentConfiguration = 5
            });

            var formAnswers = new FormAnswers
            {
                Path = "customer-pay"
            };

            var formSchema = new FormSchema
            {
                Pages = new List<Page>
                {
                    new Page
                    {
                        Behaviours = new List<Behaviour>
                        {
                            new Behaviour
                            {
                                BehaviourType = EBehaviourType.SubmitAndPay,
                                SubmitSlugs = new List<SubmitSlug>
                                {
                                    new SubmitSlug
                                    {
                                        AuthToken = "testToken",
                                        Location = "local",
                                        URL = "customer-pay"
                                    }
                                }
                            }
                        },
                        PageSlug = "customer-pay"
                    }
                }
            };

            var mappingEntity = new MappingEntity
            {
                BaseForm = formSchema,
                FormAnswers = formAnswers
            };

            var paymentProviderItems = new List<IPaymentProvider> { _paymentProvider.Object };
            _mockPaymentProvider.Setup(m => m.GetEnumerator()).Returns(() => paymentProviderItems.GetEnumerator());
            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("d96bceca-f5c6-49f8-98ff-2d823090c198");
            _mockMappingService.Setup(_ => _.Map("d96bceca-f5c6-49f8-98ff-2d823090c198", "testForm"))
                .ReturnsAsync(mappingEntity);
            _mockHostingEnvironment.Setup(_ => _.EnvironmentName).Returns("local");

            _service = new PayService(_mockPaymentProvider.Object, _mockLogger.Object, _mockGateway.Object, _mockCache.Object,
                _mockDistrbutedCacheExpirationSettings.Object, _mockSessionHelper.Object, _mockMappingService.Object, _mockHostingEnvironment.Object);
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
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPaymentResponse("nonexistanceform", "12345", "reference"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowApplicationException_WhenPaymentProvider_IsNull()
        {
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPaymentResponse("nonexistanceform", "12345", "reference"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowException_WhenPaymentProviderThrows()
        {
            _paymentProvider.Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws<Exception>();

            await Assert.ThrowsAsync<Exception>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldReturnPaymentReference_OnSuccessfull_PaymentProviderCall()
        {
            var result = await _service.ProcessPaymentResponse("testForm", "12345", "reference");

            Assert.IsType<string>(result);
            Assert.NotNull(result);
        }
    }
}