using System.Net;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.PaymentProvider;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PayService;
using form_builder.TagParsers;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class PayServiceTests
    {
        private PayService _service;
        private readonly Mock<ILogger<PayService>> _mockLogger = new();
        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<IEnumerable<IPaymentProvider>> _mockPaymentProviders = new();
        private readonly Mock<IPaymentProvider> _paymentProvider = new();
        private readonly Mock<IPaymentProvider> _fakePaymentProvider = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly Mock<IMappingService> _mockMappingService = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnvironment = new();
        private readonly Mock<IPageHelper> _mockPageHelper = new();
        private readonly Mock<IPaymentHelper> _mockPaymentHelper = new();
        private readonly Mock<IEnumerable<ITagParser>> _mockTagParsers = new();
        private readonly Mock<ITagParser> _tagParser = new();
        private readonly Mock<IOptions<PaymentConfiguration>> _mockPaymentConfiguration = new();

        public PayServiceTests()
        {
            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            _mockPageHelper
                .Setup(_ => _.GetSavedAnswers(It.IsAny<string>()))
                .Returns(new FormAnswers());

            _tagParser
                .Setup(_ => _.ParseString(It.IsAny<string>(), It.IsAny<FormAnswers>()))
                .Returns("{\"PaymentProvider\":\"testPaymentProvider\",\"Settings\":{\"CalculationSlug\":{\"Environment\":null,\"URL\":\"url\",\"Type\":\"AuthHeader\",\"AuthToken\":\"TestToken\",\"CallbackUrl\":null}}}");

            var tagParserItems = new List<ITagParser> { _tagParser.Object };

            _mockTagParsers
                .Setup(m => m.GetEnumerator())
                .Returns(() => tagParserItems.GetEnumerator());

            _paymentProvider
                .Setup(_ => _.ProviderName)
                .Returns("testPaymentProvider");

            _paymentProvider
                .Setup(_ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<PaymentInformation>()))
                .ReturnsAsync("url");

            _fakePaymentProvider
                .Setup(_ => _.ProviderName)
                .Returns("Fake");

            _fakePaymentProvider
                .Setup(_ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<PaymentInformation>()))
                .ReturnsAsync("url");

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

            var paymentProviderItems = new List<IPaymentProvider> { _paymentProvider.Object, _fakePaymentProvider.Object };

            _mockPaymentProviders
                .Setup(m => m.GetEnumerator())
                .Returns(() => paymentProviderItems.GetEnumerator());

            _mockSessionHelper
                .Setup(_ => _.GetBrowserSessionId())
                .Returns("d96bceca-f5c6-49f8-98ff-2d823090c198");

            _mockMappingService
                .Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mappingEntity);

            _mockHostingEnvironment
                .Setup(_ => _.EnvironmentName)
                .Returns("local");

            _mockPaymentConfiguration
                .Setup(_ => _.Value)
                .Returns(
                    new PaymentConfiguration
                    {
                        FakePayment = false
                    });

            _service = new PayService(_mockPaymentProviders.Object, _mockLogger.Object, _mockGateway.Object, _mockSessionHelper.Object, _mockMappingService.Object,
                _mockHostingEnvironment.Object, _mockPageHelper.Object, _mockPaymentHelper.Object, _mockPaymentConfiguration.Object, _mockTagParsers.Object);
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
        public async Task ProcessPayment_ShouldCallPageHelper()
        {
            // Act
            await _service.ProcessPayment(GetMappingEntityData(), "testForm", "page-one", "12345", "guid");

            // Assert
            _mockPageHelper.Verify(_ => _.GetSavedAnswers(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_ShouldCallPaymentHelper()
        {
            // Act
            await _service.ProcessPayment(GetMappingEntityData(), "testForm", "page-one", "12345", "guid");

            // Assert
            _mockPaymentHelper.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_ShouldCallTagParser()
        {
            // Act
            await _service.ProcessPayment(GetMappingEntityData(), "testForm", "page-one", "12345", "guid");

            // Assert
            _tagParser.Verify(_ => _.ParseString(It.IsAny<string>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_ShouldCallPaymentProvider_And_ReturnUrl()
        {
            // Act
            var result = await _service.ProcessPayment(GetMappingEntityData(), "testForm", "page-one", "12345", "guid");

            // Assert
            _paymentProvider.Verify(
                _ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<PaymentInformation>()), Times.Once);
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldThrowException_WhenPaymentProviderThrows()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation());

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldReturnPaymentReference_OnSuccessful_PaymentProviderCall()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            // Act
            var result = await _service.ProcessPaymentResponse("testForm", "12345", "reference");

            // Assert
            Assert.IsType<string>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldCallPaymentHelper_AndTagParser()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            // Act
            await _service.ProcessPaymentResponse("testForm", "12345", "reference");

            // Assert
            _mockPaymentHelper.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>()), Times.Once);
            _tagParser.Verify(_ => _.Parse(It.IsAny<Page>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldSavePaymentAmount_OnSuccessful_PaymentProviderCall()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            // Act
            await _service.ProcessPaymentResponse("testForm", "12345", "reference");

            // Assert
            _mockPageHelper.Verify(_ => _.SavePaymentAmount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldLog_Error_OnCallbackFailure_ForDeclinedJourney()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentDeclinedException("error"));

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            // Act
            await Assert.ThrowsAsync<PaymentDeclinedException>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));

            // Assert
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldLog_Error_OnCallbackFailure_ForFailureJourney()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentFailureException("error"));

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            // Act
            await Assert.ThrowsAsync<PaymentFailureException>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));

            // Assert
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostPaymentUpdateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldLog_Error_OnCallbackFailure_ForSuccessJourney()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            // Act
            await _service.ProcessPaymentResponse("testForm", "12345", "reference");

            // Assert
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostPaymentUpdateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldNotLog_Error_If_CallbackIsSuccessful_OnFailure()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentFailureException("error"));

            // Act
            await Assert.ThrowsAsync<PaymentFailureException>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));

            // Assert
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostPaymentUpdateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldNotLog_Error_If_CallbackIsSuccessful_OnDeclined()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentDeclinedException("error"));

            // Act
            await Assert.ThrowsAsync<PaymentDeclinedException>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));

            // Assert
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostPaymentUpdateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldLogError_AndNotFail_IfCallback_ThrowsException_OnSuccess_IfProcessPaymentCallbackFalse()
        {
            // Arrange
            var reference = "12345abc";
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("An error has occurred"));

            // Act
            var result = await _service.ProcessPaymentResponse("testForm", "12345", reference);

            // Assert
            Assert.Equal(reference, result);
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostPaymentUpdateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldLogError_AndThrowNormalFailedException_IfCallback_ThrowsException_OnFailure()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("An error has occured"));

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentFailureException("error"));

            // Act & Assert
            await Assert.ThrowsAsync<PaymentFailureException>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostPaymentUpdateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldLogError_AndThrowNormalDeclinedException_IfCallback_ThrowsException_OnDeclined()
        {
            // Arrange
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("An error has occured"));

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentDeclinedException("error"));

            // Act & Assert
            await Assert.ThrowsAsync<PaymentDeclinedException>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));
            _mockLogger.Verify(_ => _.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostPaymentUpdateRequest>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_Should_ThrowPaymentCallbackException_IfCallbackFails_AndProcessPaymentCallbackResponseTrue()
        {
            // Arrange
            var mappingEntity = GetMappingEntityData();
            mappingEntity.BaseForm.ProcessPaymentCallbackResponse = true;

            _mockMappingService
                .Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mappingEntity);

            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway
                .Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.FailedDependency });

            // Act & Assert
            await Assert.ThrowsAsync<PaymentCallbackException>(() => _service.ProcessPaymentResponse("testForm", "12345", "reference"));
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldCallGateway_ToProcess_PaymentResponse_OnDecline_WithCorrectModel()
        {
            // Arrange
            var reference = "REF-12345";
            PostPaymentUpdateRequest callbackModel = new();
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((a, b) => callbackModel = (PostPaymentUpdateRequest)b);

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentDeclinedException("error"));

            // Act & Assert
            await Assert.ThrowsAsync<PaymentDeclinedException>(() => _service.ProcessPaymentResponse("testForm", "12345", reference));
            Assert.Equal(reference, callbackModel.Reference);
            Assert.Equal(EPaymentStatus.Declined, callbackModel.PaymentStatus);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldCallGateway_ToProcess_PaymentResponse_OnFailure_WithCorrectModel()
        {
            // Arrange
            var reference = "REF-12345";
            PostPaymentUpdateRequest callbackModel = new();
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((a, b) => callbackModel = (PostPaymentUpdateRequest)b);

            _paymentProvider
                .Setup(_ => _.VerifyPaymentResponse(It.IsAny<string>()))
                .Throws(new PaymentFailureException("error"));

            // Act & Assert
            await Assert.ThrowsAsync<PaymentFailureException>(() => _service.ProcessPaymentResponse("testForm", "12345", reference));
            Assert.Equal(reference, callbackModel.Reference);
            Assert.Equal(EPaymentStatus.Failure, callbackModel.PaymentStatus);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentResponse_ShouldCallGateway_ToProcess_PaymentResponse_OnSuccess_WithCorrectModel()
        {
            // Arrange
            var reference = "REF-12345";
            PostPaymentUpdateRequest callbackModel = new();
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                .ReturnsAsync(new PaymentInformation
                {
                    PaymentProvider = "testPaymentProvider",
                    Settings = new Settings
                    {
                        Amount = "10"
                    }
                });

            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((a, b) => callbackModel = (PostPaymentUpdateRequest)b);

            // Act & Assert
            await _service.ProcessPaymentResponse("testForm", "12345", reference);
            Assert.Equal(reference, callbackModel.Reference);
            Assert.Equal(EPaymentStatus.Success, callbackModel.PaymentStatus);
            _mockGateway.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_ShouldCall_FakePaymentProvider_If_FakePayment_SpecifiedInOptions()
        {
            _mockPaymentConfiguration.Setup(_ => _.Value).Returns(new PaymentConfiguration { FakePayment = true });

            _service = new PayService(_mockPaymentProviders.Object, _mockLogger.Object, _mockGateway.Object, _mockSessionHelper.Object, _mockMappingService.Object,
                _mockHostingEnvironment.Object, _mockPageHelper.Object, _mockPaymentHelper.Object, _mockPaymentConfiguration.Object, _mockTagParsers.Object);
            // Act
            var result = await _service.ProcessPayment(GetMappingEntityData(), "testForm", "page-one", "12345", "guid");

            // Assert
            _fakePaymentProvider.Verify(
                _ => _.GeneratePaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<PaymentInformation>()), Times.Once);

            Assert.IsType<string>(result);
        }
    }
}