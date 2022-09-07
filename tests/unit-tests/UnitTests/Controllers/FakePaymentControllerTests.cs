using form_builder.Configuration;
using form_builder.Controllers.Payment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Controllers
{
    public class FakePaymentControllerTests
    {
        private FakePaymentController _controller;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment = new();
        private readonly Mock<IOptions<PaymentConfiguration>> _mockPaymentConfiguration = new();

        public FakePaymentControllerTests()
        {
            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Host)
                            .Returns(new HostString("www.test.com"));

            _mockWebHostEnvironment.Setup(_ => _.EnvironmentName)
                                    .Returns("local");
        }

        [Fact]
        public void Index_ShouldReturn_View_if_PaymentConfiguration_FakePayment_Is_False()
        {
            // Arrange
            _mockPaymentConfiguration.Setup(_ => _.Value).Returns(new PaymentConfiguration { FakePayment = false });

            _controller = new FakePaymentController(_mockHttpContextAccessor.Object, _mockWebHostEnvironment.Object, _mockPaymentConfiguration.Object);

            // Act
            var result = _controller.Index("form", "page-one", "0000", "123456");

            // Assert
            result = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Index_ShouldReturn_NotFound_If_PaymentConfiguration_FakePayment_Is_True()
        {
            _mockPaymentConfiguration.Setup(_ => _.Value).Returns(new PaymentConfiguration { FakePayment = true });

            // Arrange
            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Host)
                                        .Returns(new HostString("www.test.com"));

            _mockWebHostEnvironment.Setup(_ => _.EnvironmentName)
                                    .Returns("local");

            _controller = new FakePaymentController(_mockHttpContextAccessor.Object, _mockWebHostEnvironment.Object, _mockPaymentConfiguration.Object);

            // Act
            var result = _controller.Index("form", "page-one", "0000", "123456");

            // Assert
            result = Assert.IsType<ViewResult>(result);
        }
    }
}
