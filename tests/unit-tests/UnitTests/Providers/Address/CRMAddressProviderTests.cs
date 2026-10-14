namespace form_builder_tests.UnitTests.Providers.Address;

public class CRMAddressProviderTests
{
    private readonly CRMAddressProvider _addressProvider;

    private readonly Mock<IVerintServiceGateway> _mockVerintGateway = new();

    public CRMAddressProviderTests()
    {
        _mockVerintGateway.Setup(_ => _.SearchForPropertyByPostcode(It.IsAny<string>()))
            .ReturnsAsync(new List<AddressSearchResult>
            {
                new AddressSearchResult { Name = "road,city,county", UniqueId = "1234567889" }
            });

        _addressProvider = new CRMAddressProvider(_mockVerintGateway.Object);
    }

    [Fact]
    public async Task SearchAsync_ShouldCallVerintGateway()
    {
        var postcode = "sk11aa";

        await _addressProvider.SearchAsync(postcode);

        _mockVerintGateway.Verify(_ => _.SearchForPropertyByPostcode(It.Is<string>(x => x == postcode)), Times.Once);
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