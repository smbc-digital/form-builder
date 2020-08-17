using form_builder.Providers.Address;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace form_builder_tests.UnitTests.Providers.Address
{
    public class FakeProviderTests
    {
        private readonly FakeAddressProvider _addressProvider;

        public FakeProviderTests()
        {
            _addressProvider = new FakeAddressProvider();
        }

        [Fact]
        public async Task SearchAsync_ShouldReturn3AddressResults()
        {
            var postcode = "sk11aa";

            var result = await _addressProvider.SearchAsync(postcode);

            Assert.Equal(3, result.ToList().Count);
        }
    }
}
