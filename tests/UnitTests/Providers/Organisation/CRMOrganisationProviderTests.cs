using form_builder.Providers.Organisation;
using Moq;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Gateways.VerintServiceGateway;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Organisation
{
    public class CRMOrganisationProviderTests
    {
        private readonly IOrganisationProvider _organisationProvider;

        private readonly Mock<IVerintServiceGateway> _mockVerintGateway = new Mock<IVerintServiceGateway>();

        public CRMOrganisationProviderTests()
        {
            _mockVerintGateway.Setup(_ => _.SearchForOrganisationByName(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponse<List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>> { ResponseContent = new List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation> { new StockportGovUK.NetStandard.Models.Models.Verint.Organisation { Name = "org name", Reference = "1234567889" } } });

            _organisationProvider = new CRMOrganisationProvider(_mockVerintGateway.Object);
        }

        [Fact]
        public async Task SearchAsync_ShouldCallVerintGateway()
        {
            var searchTerm = "orgName";

            await _organisationProvider.SearchAsync(searchTerm);

            _mockVerintGateway.Verify(_ => _.SearchForOrganisationByName(It.Is<string>(x => x == searchTerm)), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnResponseContent()
        {
            var searchTerm = "orgName";

            var result = await _organisationProvider.SearchAsync(searchTerm);

            Assert.Single(result);
            Assert.NotNull(result);
            Assert.IsType<List<StockportGovUK.NetStandard.Models.Models.Verint.Organisation>>(result);
        }
    }
}
