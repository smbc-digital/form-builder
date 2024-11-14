using form_builder.Providers.Address;
using Moq;
using StockportGovUK.NetStandard.Gateways.Models.Addresses;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Gateways.VerintService;
using StockportGovUK.NetStandard.Gateways;
using Xunit;
using Microsoft.Extensions.Options;
using form_builder.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace form_builder_tests.UnitTests.Providers.Address
{
    public class OSPlacesAddressProviderTests
    {
        private readonly OSPlacesAddressProvider _addressProvider;

        private readonly Mock<IGateway> _mockGateway = new();
        private readonly Mock<ILogger<OSPlacesAddressProvider>> _logger = new();
        private readonly Mock<IOptions<OSPlacesAddressProviderConfiguration>> _mockOptionsConfiguration = new();  


        public OSPlacesAddressProviderTests()
        {
            _mockOptionsConfiguration
                .Setup(_ => _.Value)
                .Returns(new OSPlacesAddressProviderConfiguration
                    {
                        Key = "ReturnPlates",
                        Host = "LTXV",
                        LocalCustodianCode = "jon.chiles@stockport.gov.uk"
                    }
                );

            _mockGateway.Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new OSProperty()
                    {
                        results = new List<Result>()
                        {
                            new()
                            {
                                LPI = new()
                                {
                                    PAO_START_NUMBER = "",
                                    STREET_DESCRIPTION = "",
                                    LOCALITY_NAME = "",
                                    TOWN_NAME = "",
                                    POSTCODE_LOCATOR = "",
                                    UPRN = ""
                                }
                            }
                        }

                    }))

                });

            _addressProvider = new OSPlacesAddressProvider(_mockGateway.Object, _mockOptionsConfiguration.Object, _logger.Object);
        }

        [Fact]
        public async Task SearchAsync_ShouldCallGateway()
        {
            var postcode = "sk1 3xe";

            await _addressProvider.SearchAsync(postcode);

            _mockGateway.Verify(_ => _.GetAsync(It.IsAny<string>()), Times.Exactly(1));
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnResponseContent()
        {
            var postcode = "sk11aa";

            var result = await _addressProvider.SearchAsync(postcode);

            Assert.Single(result);
            Assert.NotNull(result);
        }
    }
}
