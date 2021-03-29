using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.Transforms.PaymentConfiguration;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PayService;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class PayServiceTests
    {
        private readonly PayService _service;
        private readonly Mock<ILogger<PayService>> _mockLogger = new Mock<ILogger<PayService>>();
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();
        private readonly Mock<IEnumerable<IPaymentProvider>> _mockPaymentProvider =
            new Mock<IEnumerable<IPaymentProvider>>();

        private readonly Mock<IPaymentProvider> _paymentProvider = new Mock<IPaymentProvider>();

        private readonly Mock<IPaymentConfigurationTransformDataProvider> _mockPaymentConfigProvider =
            new Mock<IPaymentConfigurationTransformDataProvider>();

        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IMappingService> _mockMappingService = new Mock<IMappingService>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnvironment = new Mock<IWebHostEnvironment>();
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();

        public PayServiceTests()
        {
            _paymentProvider.Setup(_ => _.ProviderName).Returns("testPaymentProvider");

            _mockPaymentConfigProvider.Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = "testForm",
                        PaymentProvider = "testPaymentProvider",
                        Settings = new Settings
                        {
                            ComplexCalculationRequired = false,
                            Amount = "12.65"
                        }
                    },
                    new PaymentInformation
                    {
                        FormName = "testFormwithnovalidpayment",
                        PaymentProvider = "invalidPaymentPorvider",
                        Settings = new Settings
                        {
                            ComplexCalculationRequired = false
                        }
                    },
                    new PaymentInformation
                    {
                        FormName = "complexCalculationForm",
                        PaymentProvider = "testPaymentProvider",
                        Settings = new Settings
                        {
                            ComplexCalculationRequired = true
                        }
                    }
                });

            var submitSlug = new SubmitSlug
            {
                AuthToken = "testToken",
                Environment = "local",
                URL = "customer-pay",
                CallbackUrl = "ddjshfkfjhk"
            };

            var formAnswers = new FormAnswers
            {
                Path = "customer-pay"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("customer-pay")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var mappingEntity = new MappingEntityBuilder()
                .WithBaseForm(formSchema)
                .WithFormAnswers(formAnswers)
                .WithData(new object())
                .Build();

            var paymentProviderItems = new List<IPaymentProvider> { _paymentProvider.Object };
            _mockPaymentProvider.Setup(m => m.GetEnumerator()).Returns(() => paymentProviderItems.GetEnumerator());
            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("d96bceca-f5c6-49f8-98ff-2d823090c198");
            _mockMappingService.Setup(_ => _.Map("d96bceca-f5c6-49f8-98ff-2d823090c198", "testForm"))
                .ReturnsAsync(mappingEntity);
            _mockMappingService.Setup(_ => _.Map("d96bceca-f5c6-49f8-98ff-2d823090c198", "nonexistanceform"))
                .ReturnsAsync(mappingEntity);
            _mockHostingEnvironment.Setup(_ => _.EnvironmentName).Returns("local");

            _service = new PayService(_mockPaymentProvider.Object, _mockLogger.Object, _mockGateway.Object, _mockSessionHelper.Object, _mockMappingService.Object,
                _mockHostingEnvironment.Object, _mockPageHelper.Object, _mockPaymentConfigProvider.Object);
        }

        private static MappingEntity GetMappingEntityData()
        {
            var submitSlug = new SubmitSlug
            {
                AuthToken = "testToken",
                Environment = "local",
                URL = "customer-pay",
                CallbackUrl = "callbackUrl"
            };

            var formAnswers = new FormAnswers
            {
                Path = "customer-pay"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("customer-pay")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var mappingEntity = new MappingEntityBuilder()
                .WithBaseForm(formSchema)
                .WithFormAnswers(formAnswers)
                .WithData(new object())
                .Build();

            return mappingEntity;
        }

        private static readonly Behaviour Behaviour = new BehaviourBuilder()
            .WithBehaviourType(EBehaviourType.SubmitAndPay)
            .WithPageSlug("test-test")
            .WithSubmitSlug(new SubmitSlug
            {
                URL = "test",
                AuthToken = "test",
                Environment = "local",
                CallbackUrl = "test"
            })
            .Build();

        private static readonly Page Page = new PageBuilder()
            .WithBehaviour(Behaviour)
            .WithPageSlug("test")
            .Build();

        [Fact]
        public async Task ProcessPayment_ShouldThrowException_WhenPaymentConfig_IsNull()
        {
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(Page);

            var result = await Assert.ThrowsAsync<Exception>(() =>
                _service.ProcessPayment(GetMappingEntityData(), "nonexistanceform", "page-one", "12345", "guid"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPayment_ShouldThrowException_WhenPaymentProvider_IsNull()
        {
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(Page);

            var result = await Assert.ThrowsAsync<Exception>(() =>
                _service.ProcessPayment(GetMappingEntityData(), "testFormwithnovalidpayment", "page-one", "12345",
                    "guid"));

            Assert.Equal("PayService::GetFormPaymentProvider, No payment provider configured for invalidPaymentPorvider", result.Message);
        }

        [Fact]
        public async Task ProcessPayment_ShouldCallPaymentProvider_And_ReturnUrl()
        {
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(Page);

            _paymentProvider.Setup(_ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<PaymentInformation>()))
                .ReturnsAsync("url");

            var result = await _service.ProcessPayment(GetMappingEntityData(), "testForm", "page-one", "12345", "guid");

            _paymentProvider.Verify(
                _ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<PaymentInformation>()), Times.Once);
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowException_WhenPaymentConfig_IsNull()
        {
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(Page);

            var result = await Assert.ThrowsAsync<Exception>(() =>
                _service.ProcessPaymentResponse("nonexistanceform", "12345", "reference"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowException_WhenPaymentProvider_IsNull()
        {
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(Page);

            var result = await Assert.ThrowsAsync<Exception>(() =>
                _service.ProcessPaymentResponse("nonexistanceform", "12345", "reference"));

            Assert.Equal("PayService:: No payment information found for nonexistanceform", result.Message);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowException_WhenPaymentProviderThrows()
        {
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(Page);

            _paymentProvider.Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws<Exception>();

            await Assert.ThrowsAsync<Exception>(() =>
                _service.ProcessPaymentResponse("testForm", "12345", "reference"));
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldReturnPaymentReference_OnSuccessfull_PaymentProviderCall()
        {
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns(Page);

            var result = await _service.ProcessPaymentResponse("testForm", "12345", "reference");

            Assert.IsType<string>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldCallGatewayIfComplexCalculationRequired()
        {
            var page = new PageBuilder().WithElement(new Element
            {
                Type = EElementType.PaymentSummary,
                Properties = new BaseProperty
                {
                    CalculationSlugs = new List<SubmitSlug>
                    {
                        new SubmitSlug
                        {
                            Environment = "local",
                            URL = "url",
                            AuthToken = "auth"
                        }
                    }
                }
            }).Build();

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("100.00")
                });

            var result = await _service.GetFormPaymentInformation(GetMappingEntityData(), "complexCalculationForm", page);

            Assert.Equal("100.00", result.Settings.Amount);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldReturnAmountFromConfig()
        {
            // Arrange
            var page = new PageBuilder().WithElement(new Element
            {
                Type = EElementType.PaymentSummary,
                Properties = new BaseProperty
                {
                    CalculationSlugs = new List<SubmitSlug>
                    {
                        new SubmitSlug
                        {
                            Environment = "local"
                        }
                    }
                }
            }).Build();

            // Act
            var result = await _service.GetFormPaymentInformation(GetMappingEntityData(), "testForm", page);

            // Assert
            Assert.Equal("12.65", result.Settings.Amount);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldThrowException_If_GatewayReturns500()
        {
            // Arrange
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var page = new PageBuilder().WithElement(new Element
            {
                Type = EElementType.PaymentSummary,
                Properties = new BaseProperty
                {
                    CalculationSlugs = new List<SubmitSlug>
                    {
                        new SubmitSlug
                        {
                            Environment = "local"
                        }
                    }
                }
            }).Build();

            // Act
            await Assert.ThrowsAsync<Exception>(() => _service.GetFormPaymentInformation(GetMappingEntityData(), "complexCalculationForm", page));
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldCallGatewayResponseIsNull()
        {
            var page = new PageBuilder().WithElement(new Element
            {
                Type = EElementType.PaymentSummary,
                Properties = new BaseProperty
                {
                    CalculationSlugs = new List<SubmitSlug>
                    {
                        new SubmitSlug
                        {
                            Environment = "local",
                            URL = "url",
                            AuthToken = "auth"
                        }
                    }
                }
            }).Build();

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = null
                });

            var result = await Assert.ThrowsAsync<Exception>(() => _service.GetFormPaymentInformation(GetMappingEntityData(), "complexCalculationForm", page));
            Assert.Equal("PayService::CalculateAmountAsync, Gateway url responded with empty payment amount within content", result.Message);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldCallGatewayResponseIsWhitespace()
        {
            var page = new PageBuilder().WithElement(new Element
            {
                Type = EElementType.PaymentSummary,
                Properties = new BaseProperty
                {
                    CalculationSlugs = new List<SubmitSlug>
                    {
                        new SubmitSlug
                        {
                            Environment = "local",
                            URL = "url",
                            AuthToken = "auth"
                        }
                    }
                }
            }).Build();

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(string.Empty)
                });

            var result = await Assert.ThrowsAsync<Exception>(() => _service.GetFormPaymentInformation(GetMappingEntityData(), "complexCalculationForm", page));
            Assert.Equal("PayService::CalculateAmountAsync, Gateway url responded with empty payment amount within content", result.Message);
        }
    }
}