using form_builder.Providers.Lookup;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Lookup
{
    public class LookupProviderTests
    {
        private readonly JsonLookupProvider _lookupProvider;
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();

        public LookupProviderTests() => _lookupProvider = new JsonLookupProvider(_mockGateway.Object);

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetAsync_ThrowError_OnNotSuccessfullyCall(HttpStatusCode httpStatusCode)
        {
            //Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            //Act
            //Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _lookupProvider.GetAsync(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        public async Task GetAsync_DoesNotThrowError_OnSuccessCall(HttpStatusCode httpStatusCode)
        {
            //Arrange
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            //Act & Assert
            var exception = await Record.ExceptionAsync(() =>  _lookupProvider.GetAsync(It.IsAny<string>(), It.IsAny<string>()));
            Assert.Null(exception);
        }
    }
}
