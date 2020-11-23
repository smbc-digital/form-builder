﻿using form_builder.Controllers;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using form_builder.Services.PayService;
using form_builder.Exceptions;
using form_builder.Helpers.Session;
using form_builder.Controllers.Payment;
using form_builder.Services.PageService;

namespace form_builder_tests.UnitTests.Controllers
{
    public class PaymentControllerTests
    {
        private PaymentController _controller;
        private readonly Mock<IPayService> _payService = new Mock<IPayService>();
        private readonly Mock<IPageService> _pageService = new Mock<IPageService>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();

        public PaymentControllerTests()
        {
            _controller = new PaymentController(_payService.Object, _pageService.Object, _sessionHelper.Object);
        }

        [Fact]
        public async Task HandlePaymentResponse_ShouldCallService_AndRedirectToSuccessAction()
        {
            _payService.Setup(_ => _.ProcessPaymentResponse(It.IsAny<string>(),It.IsAny<string>(),  It.IsAny<string>()))
                .ReturnsAsync("1234");

            var result = await _controller.HandlePaymentResponse("form", "page-one", "0000", "123456");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentSuccess", redirectResult.ActionName);
            _payService.Verify(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task HandlePaymentResponse_Should_RedirectToFailureAction_WhenPaymentFailureException()
        {
            _payService.Setup(_ => _.ProcessPaymentResponse(It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PaymentFailureException("payment failure"));

            var result = await _controller.HandlePaymentResponse("form", "page-one", "0000", "123456");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentFailure", redirectResult.ActionName);
            _payService.Verify(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task HandlePaymentResponse_Should_RedirectToDeclinedAction_WhenPaymentDeclinedException()
        {
            _payService.Setup(_ => _.ProcessPaymentResponse(It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PaymentDeclinedException("payment declined"));

            var result = await _controller.HandlePaymentResponse("form", "page-one", "0000", "123456");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PaymentDeclined", redirectResult.ActionName);
            _payService.Verify(_ => _.ProcessPaymentResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}