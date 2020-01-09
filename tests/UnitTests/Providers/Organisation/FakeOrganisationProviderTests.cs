using form_builder.Providers.Organisation;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Organisation
{
    public class FakeOrganisationProviderTests
    {
        private readonly FakeOrganisationProvider _organisationProvider;

        public FakeOrganisationProviderTests()
        {
            _organisationProvider = new FakeOrganisationProvider();
        }

        [Fact]
        public async Task SearchAsync_ShouldReturn3OrganisationResults()
        {
            var searchTerm = "orgName";

            var result = await _organisationProvider.SearchAsync(searchTerm);

            Assert.Equal(3, result.ToList().Count);
        }
    }
}
