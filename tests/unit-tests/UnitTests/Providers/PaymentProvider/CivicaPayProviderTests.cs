using System.Net;
using form_builder.Configuration;
using form_builder.Exceptions;
using form_builder.Providers.PaymentProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways.CivicaPay;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Models.Civica.Pay.Request;
using StockportGovUK.NetStandard.Models.Civica.Pay.Response;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.PaymentProvider
{
    public class CivicaPayProviderTests
    {
        private readonly IPaymentProvider _civicaPayProvider;
        private readonly Mock<ICivicaPayGateway> _mockCivicaPayGateway = new();
        private readonly Mock<IOptions<CivicaPaymentConfiguration>> _civicaPayConfig = new();
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        private readonly Mock<ILogger<CivicaPayProvider>> _logger = new();

        public CivicaPayProviderTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Scheme)
                .Returns("http");
            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Host)
                .Returns(new HostString("www.test.com"));

            _mockCivicaPayGateway.Setup(_ => _.CreateImmediateBasketAsync(It.IsAny<CreateImmediateBasketRequest>()))
                .ReturnsAsync(new HttpResponse<CreateImmediateBasketResponse> { IsSuccessStatusCode = true, StatusCode = HttpStatusCode.OK, ResponseContent = new CreateImmediateBasketResponse { BasketReference = "testRef", BasketToken = "testBasketToken", Success = "true" } });

            _civicaPayConfig.Setup(_ => _.Value).Returns(new CivicaPaymentConfiguration { CustomerId = "testId", ApiPassword = "test" });

            _civicaPayProvider = new CivicaPayProvider(_mockCivicaPayGateway.Object, _civicaPayConfig.Object, _mockHttpContextAccessor.Object, _mockHostingEnv.Object, _logger.Object);
        }

        [Fact]
        public async Task GeneratePaymentUrl_ShouldCallCivicaPayGateway()
        {
            var caseRef = "caseRef";

            await _civicaPayProvider.GeneratePaymentUrl("form", "page", caseRef, "0101010-1010101", new PaymentInformation { FormName = new[] { "form" }, Settings = new Settings() });

            _mockCivicaPayGateway.Verify(_ => _.CreateImmediateBasketAsync(It.IsAny<CreateImmediateBasketRequest>()), Times.Once);
            _mockCivicaPayGateway.Verify(_ => _.GetPaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => x == caseRef)), Times.Once);
        }


        [Fact]
        public async Task GeneratePaymentUrl_ShouldThrowException_WhenCivicaPayGatewayResponse_IsNotHttpOk()
        {
            _mockCivicaPayGateway.Setup(_ => _.CreateImmediateBasketAsync(It.IsAny<CreateImmediateBasketRequest>()))
                .ReturnsAsync(new HttpResponse<CreateImmediateBasketResponse> { StatusCode = HttpStatusCode.InternalServerError });

            var result = await Assert.ThrowsAsync<Exception>(() => _civicaPayProvider.GeneratePaymentUrl("form", "page", "ref12345", "0101010-1010101", new PaymentInformation { FormName = new[] { "form" }, Settings = new Settings() }));

            Assert.StartsWith("CivicaPayProvider::GeneratePaymentUrl, CivicaPay gateway response with a non ok status code InternalServerError, HttpResponse: ", result.Message);
        }

        [Fact]
        public async Task GeneratePaymentUrl_ShouldThrowException_WhenCivicaResponse_IsNotSuccessful()
        {
            _mockCivicaPayGateway.Setup(_ => _.CreateImmediateBasketAsync(It.IsAny<CreateImmediateBasketRequest>()))
                .ReturnsAsync(new HttpResponse<CreateImmediateBasketResponse>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true,
                    ResponseContent = new CreateImmediateBasketResponse { Success = "false" }
                });

            var result = await Assert.ThrowsAsync<Exception>(() => _civicaPayProvider.GeneratePaymentUrl("form", "page", "ref12345", "0101010-1010101", new PaymentInformation { FormName = new[] { "form" }, Settings = new Settings() }));

            Assert.StartsWith("CivicaPayProvider::GeneratePaymentUrl, CivicaPay gateway responded with a non successful response", result.Message);
        }

        [Fact]
        public async Task GeneratePaymentUrl_Should_ReturnPaymentUrl()
        {
            _mockCivicaPayGateway.Setup(_ => _.GetPaymentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("12345");

            var result = await _civicaPayProvider.GeneratePaymentUrl("form", "page", "ref12345", "0101010-1010101", new PaymentInformation { FormName = new[] { "form" }, Settings = new Settings() });

            Assert.IsType<string>(result);
            Assert.NotNull(result);
        }


        [Theory]
        [InlineData("00022")]
        [InlineData("00023")]
        public void VerifyPaymentResponse_ShouldThrowPaymentDeclinedException_OnInvalidResponseCode(string responseCode)
        {
            var result = Assert.Throws<PaymentDeclinedException>(() => _civicaPayProvider.VerifyPaymentResponse(responseCode));

            Assert.Equal($"CivicaPayProvider::Declined payment with response code: {responseCode}", result.Message);
        }

        [Theory]
        [InlineData("11111")]
        [InlineData("22222")]
        [InlineData("01010")]
        public void VerifyPaymentResponse_ShouldThrowPaymentFailureExceptionException_OnNonSuccessfulResponseCode(string responseCode)
        {
            var result = Assert.Throws<PaymentFailureException>(() => _civicaPayProvider.VerifyPaymentResponse(responseCode));

            Assert.Equal($"CivicaPayProvider::Payment failed with response code: {responseCode}", result.Message);
        }

        [Fact]
        public async Task GeneratePaymentUrl_ShouldCallCivicaPayGateway_AndPass_Name_IfSpecified()
        {
            var caseRef = "caseRef";

            await _civicaPayProvider.GeneratePaymentUrl("form", "page", caseRef, "0101010-1010101", new PaymentInformation { FormName = new[] { "form" }, Settings = new Settings { Name = "Test Name" } });

            _mockCivicaPayGateway.Verify(_ => _.CreateImmediateBasketAsync(It.Is<CreateImmediateBasketRequest>(_ => _.PaymentItems[0].AddressDetails.Name.Equals("Test Name"))));
        }

        [Fact]
        public async Task GeneratePaymentUrl_ShouldCallCivicaPayGateway_AndPass_Email_IfSpecified()
        {
            var caseRef = "caseRef";

            await _civicaPayProvider.GeneratePaymentUrl("form", "page", caseRef, "0101010-1010101", new PaymentInformation { FormName = new[] { "form" }, Settings = new Settings { Email = "test@stockport.gov.uk" } });

            _mockCivicaPayGateway.Verify(_ => _.CreateImmediateBasketAsync(It.Is<CreateImmediateBasketRequest>(_ => _.PaymentItems[0].PaymentDetails.EmailAddress.Equals("test@stockport.gov.uk"))));
        }
    }

}


