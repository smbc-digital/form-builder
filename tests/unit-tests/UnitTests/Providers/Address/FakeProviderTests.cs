using form_builder.Providers.Address;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Address;

public class FakeProviderTests
{
    private readonly FakeAddressProvider _addressProvider = new();

    [Fact]
    public async Task SearchAsync_ShouldReturn3AddressResults()
    {
        var postcode = "sk11aa";

        var result = await _addressProvider.SearchAsync(postcode);

        Assert.Equal(3, result.ToList().Count);
    }
}