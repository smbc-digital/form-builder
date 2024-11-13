using form_builder.Providers.Address;
using Moq;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Gateways.VerintService;
using StockportGovUK.NetStandard.Gateways;
using Xunit;
using Microsoft.Extensions.Options;

namespace form_builder_tests.UnitTests.Providers.Address
{
    public class OSPlacesAddressProviderTests
    {
        private readonly OSPlacesAddressProvider _addressProvider;

        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<IOptions<OSPlacesAddressProviderConfiguration>> _mockOptionsConfiguration = new();  

        public OSPlacesAddressProviderTests()
        {
            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage());

            //<List<AddressSearchResult>> { ResponseContent = new List<AddressSearchResult> { new AddressSearchResult { Name = "road,city,county", UniqueId = "10000017101" } } }

            _addressProvider = new OSPlacesAddressProvider(_mockGateway.Object, _mockOptionsConfiguration.Object);
        }

        [Fact]
        public async Task SearchAsync_ShouldCallGateway()
        {
            var postcode = "sk1 3xe";

            await _addressProvider.SearchAsync(postcode);

            _mockGateway.Verify(_ => _.GetAsync(It.Is<string>(x => x == postcode)), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnResponseContent()
        {
            var postcode = "sk11aa";

            var result = await _addressProvider.SearchAsync(postcode);

            Assert.Single(result);
            Assert.NotNull(result);
            Assert.IsType<List<AddressSearchResult>>(result);
        }
    }
}
