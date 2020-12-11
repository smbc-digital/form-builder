using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Booking;
using form_builder.Providers.Booking;
using form_builder.Providers.StorageProvider;
using form_builder.Services.BookingService;
using form_builder.Services.BookingService.Entities;
using form_builder.Services.MappingService;
using form_builder_tests.Builders;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.Booking.Response;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class BookingServiceTests
    {
        private readonly BookingService _service;
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();
        private readonly IEnumerable<IBookingProvider> _bookingProviders;
        private readonly Mock<IBookingProvider> _bookingProvider = new Mock<IBookingProvider>();
        private readonly Mock<IPageFactory> _mockPageFactory = new Mock<IPageFactory>();
        private readonly Mock<IMappingService> _mockMappingService = new Mock<IMappingService>();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockdistributedCacheExpirationConfiguration = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();
        private readonly DistributedCacheExpirationConfiguration _cacheConfig = new DistributedCacheExpirationConfiguration { Booking = 5, BookingNoAppointmentsAvailable = 10 };

        public BookingServiceTests()
        {
            _mockdistributedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(_cacheConfig);

            _bookingProvider.Setup(_ => _.ProviderName).Returns("testBookingProvider");
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
              _mockdistributedCacheExpirationConfiguration.Object);
        }

        [Fact]
        public async Task Get_ShouldRetrieve_BookingInformation_FromFormData_WhenStoredInCache()
        {
            var bookingInfo = new BookingInformation();

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers{ FormData = new Dictionary<string, object> { {$"bookingQuestion{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}", bookingInfo } } }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("bookingQuestion")
                .Build();
            
            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = await _service.Get("form", page, "guid");
            
            Assert.IsType<BookingProcessEntity>(result);
            Assert.NotNull(result.BookingInfo);
            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))), Times.Once);
        }

        [Fact]
        public async Task Get_Should_Call_NextAvailability_And_GetAvailability_OnBookingProvdier_WhenDataNotInCache_And_SaveInCacheForFutureUse()
        {
            var guid = Guid.NewGuid();
            var date = DateTime.Today.AddDays(-2);

            _bookingProvider.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new AvailabilityDayResponse { Date = date });

            _bookingProvider.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new List<AvailabilityDayResponse> { new AvailabilityDayResponse() });

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers{ FormData = new Dictionary<string, object>() }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(guid)
                .Build();
            
            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = await _service.Get("form", page, "guid");
            
            Assert.IsType<BookingProcessEntity>(result);
            Assert.False(result.BookingHasNoAvailableAppointments);
            var listOfObjects = Assert.IsType<List<object>>(result.BookingInfo);
            var bookingInfo = Assert.IsType<BookingInformation>(listOfObjects.First());
            Assert.False(bookingInfo.IsFullDayAppointment);
            Assert.Equal(date, bookingInfo.CurrentSearchedMonth);
            Assert.Equal(date, bookingInfo.FirstAvailableMonth);
            Assert.Single(bookingInfo.Appointments);

            _bookingProvider.Verify(_ => _.NextAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _bookingProvider.Verify(_ => _.GetAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(_ => _.Equals($"testBookingProvider-{guid}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(_cacheConfig.Booking)), It.IsAny<CancellationToken>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.Is<string>(_ => _.Equals($"{element.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}")), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Get_Should_Call_NextAvailability_Only_WhenProvider_Throws_BookingNoAvailabilityException()
        {
            var guid = Guid.NewGuid();

            _bookingProvider.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .Throws(new BookingNoAvailabilityException("BookingNoAvailabilityException"));

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers{ FormData = new Dictionary<string, object>() }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(guid)
                .Build();
            
            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = await _service.Get("form", page, "guid");
            
            Assert.IsType<BookingProcessEntity>(result);
            Assert.True(result.BookingHasNoAvailableAppointments);
            _bookingProvider.Verify(_ => _.NextAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _bookingProvider.Verify(_ => _.GetAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Never);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(_ => _.Equals($"testBookingProvider-{guid}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(_cacheConfig.BookingNoAppointmentsAvailable)), It.IsAny<CancellationToken>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task Get_Should_NotCall_NextAvailability_OnBookingProvdier_WhenBookingInformationIs_InCache()
        {
            var guid = Guid.NewGuid();

            _bookingProvider.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new AvailabilityDayResponse());

            _bookingProvider.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new List<AvailabilityDayResponse>());

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object>() }));

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals($"testBookingProvider-{guid}"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new BoookingNextAvailabilityEntity {  DayResponse = new AvailabilityDayResponse() }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(guid)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = await _service.Get("form", page, "guid");

            Assert.IsType<BookingProcessEntity>(result);
            Assert.False(result.BookingHasNoAvailableAppointments);
            _bookingProvider.Verify(_ => _.NextAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Never);
            _bookingProvider.Verify(_ => _.GetAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(_ => _.Equals($"testBookingProvider-{guid}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(_cacheConfig.Booking)), It.IsAny<CancellationToken>()), Times.Never);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.Is<string>(_ => _.Equals($"{element.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}")), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Get_Should_Set_AppointmentStartTime_AndAppointmentEndTime_WhenFullDayAppointment()
        {
            var guid = Guid.NewGuid();
            var startTime = new TimeSpan(6, 0, 0);
            var endTime = new TimeSpan(19, 0, 0);
            _bookingProvider.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new AvailabilityDayResponse());

            _bookingProvider.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new List<AvailabilityDayResponse> { new AvailabilityDayResponse() });

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object>() }));

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals($"testBookingProvider-{guid}"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new BoookingNextAvailabilityEntity { DayResponse = new AvailabilityDayResponse { AppointmentTimes = new List<AppointmentTime> { new AppointmentTime { StartTime = startTime, EndTime = endTime } } } }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(guid)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = await _service.Get("form", page, "guid");

            Assert.IsType<BookingProcessEntity>(result);
            var listOfObjects = Assert.IsType<List<object>>(result.BookingInfo);
            var bookingInfo = Assert.IsType<BookingInformation>(listOfObjects.First());
            Assert.True(bookingInfo.IsFullDayAppointment);
            Assert.Equal(DateTime.Today.Add(startTime), bookingInfo.AppointmentStartTime);
            Assert.Equal(DateTime.Today.Add(endTime), bookingInfo.AppointmentEndTime);
            Assert.Single(bookingInfo.Appointments);
        }
    }
}
