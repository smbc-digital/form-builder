using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.Transforms.PaymentConfiguration;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class PaymentConfigurationCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        private readonly Mock<IPaymentProvider> _mockPaymentProvider = new();
        private readonly Mock<IEnumerable<IPaymentProvider>> _mockPaymentProviders = new();
        private readonly Mock<IPaymentConfigurationTransformDataProvider> _mockPaymentConfigProvider = new();

        public PaymentConfigurationCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("local");

            _mockPaymentProvider.Setup(_ => _.ProviderName).Returns("testProvider");
            var paymentProviderItems = new List<IPaymentProvider> { _mockPaymentProvider.Object };
            _mockPaymentProviders.Setup(m => m.GetEnumerator()).Returns(() => paymentProviderItems.GetEnumerator());
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsNotValid_WhenNoConfigFound_ForForm()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>());

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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object,
                _mockPaymentProviders.Object,
                _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsNotValid_WhenConfigFound_ForForm_ButStaticAmountAndCalculationSlugsAreNotSet()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation { FormName = new[] {"test-name"}, PaymentProvider = "testProvider", Settings = new Settings() }
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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsNotValid_WhenConfigFound_ForForm_ServicePayReferenceSet_But_ServicePayNarrativeNotSet()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation {
                        FormName = new[] {"test-name"},
                        PaymentProvider = "testProvider",
                        Settings = new Settings
                        {
                            Amount = "10",
                            ServicePayReference = "ServicePayReference"
                        }
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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsNotValid_WhenConfigFound_ForForm_ServicePayNarrativeSet_But_ServicePayReferenceNotSet()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation {
                        FormName = new[] {"test-name"},
                        PaymentProvider = "testProvider",
                        Settings = new Settings
                        {
                            Amount = "10",
                            ServicePayNarrative = "ServicePayNarrative"
                        }
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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsValid_WhenConfigFound_ForForm_WithProviderAndStaticAmount()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation { FormName = new[] {"test-name"}, PaymentProvider = "testProvider", Settings = new Settings { Amount = "10.00"} }
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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsValid_WhenConfigFound_ForForm_WithProviderAndCalculationSlugs()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = new[] {"test-name"},
                        PaymentProvider = "testProvider",
                        Settings = new Settings
                        {
                            CalculationSlug = new SubmitSlug
                            {
                                URL = "https://",
                                AuthToken = "token"
                            }
                        }
                    }
                });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task PaymentConfigurationCheck_Should_VerifyCalculationSlugs_StartWithHttps()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = new[] {"test-name"},
                        PaymentProvider = "testProvider",
                        Settings = new Settings
                        {
                            CalculationSlug = new SubmitSlug
                            {
                                URL = "https://",
                                AuthToken = "token"
                            }
                        }
                    }
                });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_Should_ThrowException_WhenCalculateCostUrl_DoesNot_StartWithHttps()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = new[] {"test-name"},
                        PaymentProvider = "testProvider",
                        Settings = new Settings
                        {
                            CalculationSlug = new SubmitSlug
                            {
                                URL = "http://",
                                Environment = "non-local",
                                AuthToken = "token"
                            }
                        }
                    }
                });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsNotValid_WhenPaymentProvider_DoesNotExists_WhenConfig_IsFound()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation { FormName = new[] {"test-name"}, PaymentProvider = "testProvider", Settings = new Settings {  } }
                });

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

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
        [Fact]
        public async Task PaymentConfigurationCheck_IsInValid_WhenQuestionIdNotFound_InForm_ForAddressReference()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = new[] {"test-name"},
                        PaymentProvider = "testProvider",
                        Settings = new Settings
                        {
                            Amount = "10",
                            AddressReference = "addressReference"
                        }
                    }
                });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var addressElement = new AddressBuilder()
                .WithQuestionId("test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithElement(addressElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .WithBaseUrl("test-name")
                .Build();

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.Collection<string>(result.Messages, message => Assert.Contains("PaymentConfiguration::AddressReference QuestionId on Address element must match with AddressReference in payment-config", message));
        }

        [Fact]
        public async Task PaymentConfigurationCheck_IsValid_ForAddressReference_When_QuestionIdMatches()
        {
            // Arrange
            _mockPaymentConfigProvider
                .Setup(_ => _.Get<List<PaymentInformation>>())
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = new[] {"test-name"},
                        PaymentProvider = "testProvider",
                        Settings = new Settings
                        {
                            Amount = "10",
                            AddressReference = "addressReference"
                        }
                    }
                });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var addressElement = new AddressBuilder()
                .WithQuestionId("addressReference")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithElement(addressElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithName("test-name")
                .WithBaseUrl("test-name")
                .Build();

            var check = new PaymentConfigurationCheck(_mockHostingEnv.Object, _mockPaymentProviders.Object, _mockPaymentConfigProvider.Object);

            // Act
            var result = await check.ValidateAsync(schema);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}