using System.Net;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.Transforms.PaymentConfiguration;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class PaymentHelperTests
    {
        private readonly PaymentHelper _paymentHelper;
        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<IPaymentConfigurationTransformDataProvider> _mockPaymentConfigProvider = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly Mock<IMappingService> _mockMappingService = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnvironment = new();

        private readonly MappingEntity _mappingEntity;

        public PaymentHelperTests()
        {
            _mockPaymentConfigProvider.Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new()
                    {
                        FormName = new[] {"testForm"},
                        PaymentProvider = "testPaymentProvider",
                        Settings = new Settings
                        {
                            Amount = "12.65",
                            AddressReference = "{{addressReference}}"
                        }
                    },
                    new()
                    {
                        FormName = new[] {"testFormWithNoValidPayment"},
                        PaymentProvider = "invalidPaymentProvider",
                        Settings = new Settings
                        {
                            Amount = "10.00"
                        }
                    },
                    new()
                    {
                        FormName = new[] {"complexCalculationForm"},
                        PaymentProvider = "testPaymentProvider",
                        Settings = new Settings
                        {
                            CalculationSlug =  new SubmitSlug
                            {
                                URL = "url",
                                Environment = "local",
                                AuthToken = "token"
                            }
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

            _mappingEntity = new MappingEntityBuilder()
                .WithBaseForm(formSchema)
                .WithFormAnswers(formAnswers)
                .WithData(new object())
                .Build();

            _mockSessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("d96bceca-f5c6-49f8-98ff-2d823090c198");
            _mockMappingService.Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(_mappingEntity);
            _mockHostingEnvironment.Setup(_ => _.EnvironmentName).Returns("local");
            _paymentHelper = new PaymentHelper(_mockGateway.Object, _mockSessionHelper.Object, _mockMappingService.Object, _mockHostingEnvironment.Object, _mockPaymentConfigProvider.Object);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldCallIPaymentConfigurationTransformProvider()
        {
            // Act
            await _paymentHelper.GetFormPaymentInformation(new FormAnswers { FormName = "testForm" }, new FormSchema());

            // Assert
            _mockPaymentConfigProvider.Verify(_ => _.Get<List<PaymentInformation>>(), Times.Once);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldCallGatewayIfComplexCalculationRequired()
        {
            // Arrange
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("100.00")
                });

            // Act
            var result = await _paymentHelper.GetFormPaymentInformation(new FormAnswers { FormName = "complexCalculationForm" }, new FormSchema());

            // Assert
            Assert.Equal("100.00", result.Settings.Amount);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldReturnAmountFromConfig()
        {
            // Act
            var result = await _paymentHelper.GetFormPaymentInformation(new FormAnswers { FormName = "testForm" }, new FormSchema());

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

            // Act
            await Assert.ThrowsAsync<Exception>(() => _paymentHelper.GetFormPaymentInformation(new FormAnswers { FormName = "complexCalculationForm" }, new FormSchema()));
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldThrowException_If_GatewayResponseIsNull()
        {
            // Arrange
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = null
                });

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => _paymentHelper.GetFormPaymentInformation(new FormAnswers { FormName = "complexCalculationForm" }, new FormSchema()));

            // Assert
            Assert.Equal("PayService::CalculateAmountAsync, Gateway url responded with empty payment amount within content", result.Message);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldThrowException_If_GatewayResponseIsWhitespace()
        {
            // Arrange
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(string.Empty)
                });

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => _paymentHelper.GetFormPaymentInformation(new FormAnswers { FormName = "complexCalculationForm" }, new FormSchema()));

            // Assert
            Assert.Equal("PayService::CalculateAmountAsync, Gateway url responded with empty payment amount within content", result.Message);
        }

        [Fact]
        public async Task GetFormPaymentInformation_ShouldAddSuffixToAddress()
        {
            // Act
            var result = await _paymentHelper.GetFormPaymentInformation(new FormAnswers { FormName = "testForm" }, new FormSchema());

            // Assert
            Assert.Equal("{{addressReference-address-description}}", result.Settings.AddressReference);
        }
    }
}

