using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.PaymentProvider;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class PaymemtConfigurationCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        private readonly Mock<ICache> _mockCache = new Mock<ICache>();

        private readonly Mock<IPaymentProvider> _mockPaymentProvider = new Mock<IPaymentProvider>();

        private readonly Mock<IEnumerable<IPaymentProvider>> _mockPaymentProviders = new Mock<IEnumerable<IPaymentProvider>>();

        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationSettings
            = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();

        public PaymemtConfigurationCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("local");

            _mockCache.Setup(_ =>
                    _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(),
                        It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = "test-name",
                        PaymentProvider = "testProvider"
                    },
                    new PaymentInformation
                    {
                        FormName = "test-form-with-incorrect-provider",
                        PaymentProvider = "invalidProvider"
                    }
                });

            _mockPaymentProvider.Setup(_ => _.ProviderName).Returns("testProvider");
            var paymentProviderItems = new List<IPaymentProvider> { _mockPaymentProvider.Object };
            _mockPaymentProviders.Setup(m => m.GetEnumerator()).Returns(() => paymentProviderItems.GetEnumerator());

            _mockDistributedCacheExpirationSettings.Setup(_ => _.Value).Returns(
                new DistributedCacheExpirationConfiguration
                {
                    UserData = 30,
                    PaymentConfiguration = 5,
                    FileUpload = 60
                });
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsNotValid_WhenNoConfigFound_ForForm()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .Build();

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockCache.Object, _mockPaymentProviders.Object, _mockDistributedCacheExpirationSettings.Object);

            // Assert
            var result = await check.ValidateAsync(schema);
            Assert.False(result.IsValid);
            Assert.Contains(result.Messages, message => message.Equals($"FAILURE - No payment information configured for 'test-name' form"));
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsValid_WhenConfigFound_ForForm_WithProvider()
        {
            // Arrange
            _mockCache
                .Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = "test-name",
                        PaymentProvider = "testProvider",
                        Settings = new Settings()
                    }
                });

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .WithBaseUrl("test-name")
                .Build();

            // Act
            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockCache.Object, _mockPaymentProviders.Object, _mockDistributedCacheExpirationSettings.Object);

            // Assert
            var result = await check.ValidateAsync(schema);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task PaymentConfigurationCheck_Should_VerifyCalculationSlugs_StartWithHttps()
        {
            // Arrange
            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation> { new PaymentInformation { FormName = "test-name", PaymentProvider = "testProvider", Settings = new Settings { ComplexCalculationRequired = true } } });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var element = new ElementBuilder()
                .WithType(EElementType.PaymentSummary)
                .WithCalculationSlugs(new SubmitSlug { Environment = "non-local", URL = "https://www.test.com" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .WithBaseUrl("test-name")
                .Build();

            // Act
            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockCache.Object, _mockPaymentProviders.Object, _mockDistributedCacheExpirationSettings.Object);

            // Assert
            var result = await check.ValidateAsync(schema);
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_Should_ThrowException_WhenCalculateCostUrl_DoesNot_StartWithHttps()
        {
            // Arrange
            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation> { new PaymentInformation { FormName = "test-name", PaymentProvider = "testProvider", Settings = new Settings { ComplexCalculationRequired = true } } });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var element = new ElementBuilder()
                .WithType(EElementType.PaymentSummary)
                .WithCalculationSlugs(new SubmitSlug { Environment = "non-local", URL = "http://www.test.com" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .WithBaseUrl("test-name")
                .Build();

            // Act
            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockCache.Object, _mockPaymentProviders.Object, _mockDistributedCacheExpirationSettings.Object);

            // Assert
            var result = await check.ValidateAsync(schema);
            Assert.Contains(result.Messages, message => message.Equals($"FAILURE - PaymentSummary::CalculateCostUrl must start with https"));
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task
        PaymentConfigurationCheck_IsNotValid_WhenPaymentProvider_DoesNotExists_WhenConfig_IsFound()
        {
            // Arrange
            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-form-with-incorrect-provider")
                .WithBaseUrl("test-form-with-incorrect-provider")
                .Build();     

            // Act
            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockCache.Object, _mockPaymentProviders.Object, _mockDistributedCacheExpirationSettings.Object);

            // Assert
            var result = await check.ValidateAsync(schema);
            Assert.False(result.IsValid);       
            Assert.Contains(result.Messages, message => message.Equals($"FAILURE - No payment provider configured for provider 'invalidProvider'"));
        }
    }
}