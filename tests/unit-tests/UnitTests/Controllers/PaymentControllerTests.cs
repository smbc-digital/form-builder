using form_builder.Controllers.Payment;
using form_builder.Exceptions;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PayService;
using form_builder.Workflows.SuccessWorkflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly PaymentController _controller;
        private readonly Mock<IPayService> _payService = new();
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<IMappingService> _mappingService = new();
        private readonly Mock<ISuccessWorkflow> _successWorkflow = new();
        private readonly Mock<ILogger<PaymentController>> _logger = new();

        public PaymentControllerTests()
        {
            _sessionHelper
                .Setup(_ => _.GetBrowserSessionId())
                .Returns("123");

            _mappingService
                .Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MappingEntity
                {
                    BaseForm = new FormSchema
                    {
                        FormName = "form",
                        StartPageUrl = "url",
                        CallbackFailureContactNumber = "phone"
                    }
                });

            _controller = new PaymentController(_payService.Object, _sessionHelper.Object, _mappingService.Object, _successWorkflow.Object, _logger.Object);
        }

        [Fact]
        public async Task HandlePaymentResponse_ShouldCallService_AndRedirectToSuccessAction()
        {
            // Arrange
            _payService.Setup(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("1234");

            // Act
            var result = await _controller.HandlePaymentResponse("form", "page-one", "0000", "123456");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentSuccess", redirectResult.ActionName);
            _payService.Verify(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task HandlePaymentResponse_Should_RedirectToFailureAction_WhenPaymentFailureException()
        {
            // Arrange
            _payService.Setup(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PaymentFailureException("payment failure"));

            // Act
            var result = await _controller.HandlePaymentResponse("form", "page-one", "0000", "123456");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentFailure", redirectResult.ActionName);
            _payService.Verify(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task HandlePaymentResponse_Should_RedirectToDeclinedAction_WhenPaymentDeclinedException()
        {
            // Arrange
            _payService.Setup(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PaymentDeclinedException("payment declined"));

            // Act
            var result = await _controller.HandlePaymentResponse("form", "page-one", "0000", "123456");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentDeclined", redirectResult.ActionName);
            _payService.Verify(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task HandlePaymentResponse_Should_RedirectToCallbackFailureAction_WhenPaymentCallbackException()
        {
            // Arrange
            _payService.Setup(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PaymentCallbackException("callback failure"));

            // Act
            var result = await _controller.HandlePaymentResponse("form", "page-one", "0000", "123456");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CallbackFailure", redirectResult.ActionName);
        }

        [Fact]
        public async Task CallbackFailure_Should_ReturnView()
        {
            // Act
            var result = await _controller.CallbackFailure("form", "123456");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("./CallbackFailure", viewResult.ViewName);
        }

        [Fact]
        public async Task CallbackFailure_Should_CallSessionHelper()
        {
            // Act
            await _controller.CallbackFailure("form", "123456");

            // Assert
            _sessionHelper.Verify(_ => _.GetBrowserSessionId(), Times.Once);
        }

        [Fact]
        public async Task CallbackFailure_Should_CallMappingService()
        {
            // Act
            await _controller.CallbackFailure("form", "123456");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
