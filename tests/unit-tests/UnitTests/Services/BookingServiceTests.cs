using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.ContentFactory;
using form_builder.Helpers.PageHelpers;
using form_builder.Providers.Booking;
using form_builder.Providers.StorageProvider;
using form_builder.Services.BookingService;
using form_builder.Services.MappingService;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class BookingServiceTests
    {
        private readonly BookingService _service;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache;
        private readonly Mock<IPageHelper> _mockPageHelper;
        private readonly IEnumerable<IBookingProvider> _bookingProviders;
        private readonly Mock<IBookingProvider> _bookingProvider = new Mock<IBookingProvider>();
        private readonly Mock<IPageFactory> _mockPageFactory;
        private readonly Mock<IMappingService> _mockMappingService;
        private readonly IOptions<DistributedCacheExpirationConfiguration> _distributedCacheExpirationConfiguration;

        public BookingServiceTests()
        {
            _bookingProvider.Setup(_ => _.ProviderName).Returns("testAddressProvider");
            _bookingProviders = new List<IBookingProvider>
            {
                _bookingProvider.Object
            };

            _service = new BookingService(
              _mockDistributedCache.Object,
              _mockPageHelper.Object,
              _bookingProviders,
              _mockPageFactory.Object,
              _mockMappingService.Object,
              _distributedCacheExpirationConfiguration);
        }


        [Fact]
        public async Task Get_()
        {
            
        }
    }
}
