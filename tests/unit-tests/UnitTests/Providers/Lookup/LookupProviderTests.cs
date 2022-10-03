using System.Net;
using form_builder.Models;
using form_builder.Providers.Lookup;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Lookup
{
    public class LookupProviderTests
    {
        private readonly JsonLookupProvider _lookupProvider;
        private readonly Mock<IGateway> _mockGateway = new();

        public LookupProviderTests() => _lookupProvider = new JsonLookupProvider(_mockGateway.Object);

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetAsync_ThrowError_OnNotSuccessfullyCall(HttpStatusCode httpStatusCode)
        {
            // Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _lookupProvider.GetAsync(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        public async Task GetAsync_DoesNotThrowError_OnSuccessCall(HttpStatusCode httpStatusCode)
        {
            // Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _lookupProvider.GetAsync(It.IsAny<string>(), It.IsAny<string>()));
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetAsync_Should_ReturnOptionsResponse()
        {
            // Arrange
            var optionsResponse = new OptionsResponse
            {
                Options = new List<Option>
                {
                    new()
                    {
                        Text = "Text",
                        Value = "Value"
                    }
                }
            };

            _mockGateway
                .Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(optionsResponse))
                });

            // Act
            var result = await _lookupProvider.GetAsync("url", "token");

            // Assert
            Assert.IsType<OptionsResponse>(result);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetAppointmentTypesAsync_ThrowError_OnNotSuccessfullyCall(HttpStatusCode httpStatusCode)
        {
            // Arrange
            _mockGateway
                .Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _lookupProvider.GetAppointmentTypesAsync(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        public async Task GetAppointmentTypesAsync_DoesNotThrowError_OnSuccessCall(HttpStatusCode httpStatusCode)
        {
            // Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _lookupProvider.GetAppointmentTypesAsync(It.IsAny<string>(), It.IsAny<string>()));
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetAppointmentTypesAsync_Should_ReturnListOfAppointmentTypes()
        {
            // Arrange
            var appointmentTypesResponse = new List<AppointmentType>
            {
                new()
                {
                    AppointmentId = Guid.NewGuid(),
                    Environment = "local"
                }
            };

            _mockGateway
                .Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(appointmentTypesResponse))
                });

            // Act
            var result = await _lookupProvider.GetAppointmentTypesAsync("url", "token");

            // Assert
            Assert.IsType<List<AppointmentType>>(result);
        }
    }
}
