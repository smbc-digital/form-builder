using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Booking;
using form_builder.Models.Elements;
using form_builder.Providers.Booking;
using form_builder.Providers.StorageProvider;
using form_builder.Services.BookingService;
using form_builder.Services.BookingService.Entities;
using form_builder.Services.MappingService;
using form_builder.Services.PageService.Entities;
using form_builder.Utils.Hash;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();
        private readonly DistributedCacheExpirationConfiguration _cacheConfig = new DistributedCacheExpirationConfiguration { Booking = 5, BookingNoAppointmentsAvailable = 10 };
        private readonly Mock<ISchemaFactory> _schemaFactory = new Mock<ISchemaFactory>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IHashUtil> _hashUtil = new Mock<IHashUtil>();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new Mock<IHttpContextAccessor>();
        

        public BookingServiceTests()
        {
            _mockdistributedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(_cacheConfig);
            _mockMappingService
                .Setup(_ => _.MapBookingRequest(It.IsAny<string>(), It.IsAny<IElement>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .ReturnsAsync(new BookingRequest { Customer = new Customer() });
            _bookingProvider.Setup(_ => _.ProviderName).Returns("testBookingProvider");
            _bookingProviders = new List<IBookingProvider>
            {
                _bookingProvider.Object
            };

            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("test");

            _service = new BookingService(
              _mockDistributedCache.Object,
              _mockPageHelper.Object,
              _bookingProviders,
              _mockPageFactory.Object,
              _mockMappingService.Object,
              _mockHostingEnv.Object,
              _schemaFactory.Object,
              _sessionHelper.Object,
              _hashUtil.Object,
              _mockdistributedCacheExpirationConfiguration.Object,
              _httpContextAccessor.Object);
        }

        [Fact]
        public async Task Get_ShouldRetrieve_BookingInformation_FromFormData_WhenStoredInCache()
        {
            var bookingInfo = new BookingInformation();

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object> { { $"bookingQuestion{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}", bookingInfo } } }));

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
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object>() }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
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
            Assert.Equal(new DateTime(date.Year, date.Month, 1), bookingInfo.CurrentSearchedMonth);
            Assert.Equal(new DateTime(date.Year, date.Month, 1), bookingInfo.FirstAvailableMonth);
            Assert.Single(bookingInfo.Appointments);

            _bookingProvider.Verify(_ => _.NextAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _bookingProvider.Verify(_ => _.GetAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(_ => _.Equals($"testBookingProvider-{guid}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(_cacheConfig.Booking)), It.IsAny<CancellationToken>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.Is<string>(_ => _.Equals($"{element.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}")), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Get_Should_Save_NextAvailability_InCache_Using_Valid_Key_With_OptionalResources_WhenSupplied()
        {
            var guid = Guid.NewGuid();
            var date = DateTime.Today.AddDays(-2);
            var bookingResourceId = Guid.NewGuid();
            var bookingResourceQuantity = 3;

            _bookingProvider.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new AvailabilityDayResponse { Date = date });

            _bookingProvider.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new List<AvailabilityDayResponse> { new AvailabilityDayResponse() });

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object>() }));

            var appointmentType = new AppointmentTypeBuilder()
                .WithAppointmentId(guid)
                .WithOptionalResource(new BookingResource { ResourceId = bookingResourceId, Quantity = bookingResourceQuantity })
                .Build();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(appointmentType)
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
            Assert.Equal(new DateTime(date.Year, date.Month, 1), bookingInfo.CurrentSearchedMonth);
            Assert.Equal(new DateTime(date.Year, date.Month, 1), bookingInfo.FirstAvailableMonth);
            Assert.Single(bookingInfo.Appointments);

            _bookingProvider.Verify(_ => _.NextAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _bookingProvider.Verify(_ => _.GetAvailability(It.Is<AvailabilityRequest>(_ => _.AppointmentId.Equals(guid))), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(_ => _.Equals($"testBookingProvider-{guid}-{bookingResourceQuantity}{bookingResourceId}")), It.IsAny<string>(), It.Is<int>(_ => _.Equals(_cacheConfig.Booking)), It.IsAny<CancellationToken>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.Is<string>(_ => _.Equals($"{element.Properties.QuestionId}{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}")), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Get_Should_Call_NextAvailability_Only_WhenProvider_Throws_BookingNoAvailabilityException()
        {
            var guid = Guid.NewGuid();

            _bookingProvider.Setup(_ => _.NextAvailability(It.IsAny<AvailabilityRequest>()))
                .Throws(new BookingNoAvailabilityException("BookingNoAvailabilityException"));

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object>() }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
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
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new BoookingNextAvailabilityEntity { DayResponse = new AvailabilityDayResponse() }));

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
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
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
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

        [Fact]
        public async Task ProcessMonthRequest_ShouldThrow_ApplicationException_WhenViewModelDoes_NotContainRequestedMonth()
        {
            // Act
            var guid = Guid.NewGuid();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("path")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("form")
                .WithPage(page)
                .Build();

            _schemaFactory.Setup(_ => _.Build("form"))
                .ReturnsAsync(formSchema);

            // Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessMonthRequest(new Dictionary<string, object>(), "form", "path"));

            _bookingProvider.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Never);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
            Assert.Equal("BookingService::ProcessMonthRequest, request for appointment did not contain requested month", result.Message);
        }

        [Fact]
        public async Task ProcessMonthRequest_ShouldThrow_ApplicationException_When_RequestedMonth_IsGreater_ThenAllowedSearchPeriod()
        {
            // Act
            var guid = Guid.NewGuid();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("path")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .WithPage(page)
                .Build();

            _schemaFactory.Setup(_ => _.Build("base-form"))
                .ReturnsAsync(formSchema);

            var model = new Dictionary<string, object>{
                { BookingConstants.BOOKING_MONTH_REQUEST, DateTime.Now.AddYears(1).AddMonths(1).ToString() }
            };

            // Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessMonthRequest(model, "base-form", "path"));

            _bookingProvider.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Never);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
            Assert.Equal("BookingService::ProcessMonthRequest, Invalid request for appointment search, Start date provided is after allowed search period", result.Message);
        }

        [Fact]
        public async Task ProcessMonthRequest_ShouldThrow_ApplicationException_When_RequestedMonth_IsBefore_Today()
        {
            // Act
            var guid = Guid.NewGuid();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .WithPage(page)
                .Build();

            _schemaFactory.Setup(_ => _.Build("base-form"))
                .ReturnsAsync(formSchema);

            var model = new Dictionary<string, object>{
                { BookingConstants.BOOKING_MONTH_REQUEST, DateTime.Now.AddMonths(-1).ToString() }
            };

            // Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessMonthRequest(model, "base-form", "page-one"));

            _bookingProvider.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Never);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
            Assert.Equal("BookingService::ProcessMonthRequest, Invalid request for appointment search, Start date provided is before today", result.Message);
        }

        [Fact]
        public async Task ProcessMonthRequest_ShouldCall_BookingProvider_AndSaveUpdatedBooking_Information()
        {
            var guid = Guid.NewGuid();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = guid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("path")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("form")
                .WithPage(page)
                .Build();

            var bookingInformationCacheKey = $"bookingQuestion{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}";
            _bookingProvider.Setup(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()))
                .ReturnsAsync(new List<AvailabilityDayResponse> { new AvailabilityDayResponse() });

            _sessionHelper.Setup(_ => _.GetSessionGuid())
                .Returns("guid");

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object> { { bookingInformationCacheKey, new BookingInformation() } } }));

            _schemaFactory.Setup(_ => _.Build("form"))
                .ReturnsAsync(formSchema);

            var model = new Dictionary<string, object>{
                { BookingConstants.BOOKING_MONTH_REQUEST, DateTime.Now.ToString() }
            };

            await _service.ProcessMonthRequest(model, "form", "path");

            _bookingProvider.Verify(_ => _.GetAvailability(It.IsAny<AvailabilityRequest>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.Is<string>(_ => _.Equals("guid")), It.Is<string>(_ => _.Equals("form"))), Times.Once);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async void ProcessMonthRequest_Should_Throw_ApplicationException_When_InvalidForm()
        {
            //
            _schemaFactory.Setup(_ => _.Build(It.IsAny<string>()))
                .ReturnsAsync((FormSchema)null);

            // Arrange
            const string form = "invalid";
            const string path = "irrelevant";
            var viewModel = new Dictionary<string, object>();

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessMonthRequest(viewModel, form, path));
            Assert.Equal($"Requested form '{form}' could not be found.", result.Message);
            _sessionHelper.Verify(_ => _.GetSessionGuid(), Times.Never);
        }


        [Fact]
        public async void ProcessMonthRequest_Should_Should_Throw_ApplicationException_When_InvalidPath()
        {
            // Arrange
            const string form = "base-form";
            const string path = "invalid";

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = Guid.NewGuid(), Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("valid")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl(form)
                .Build();

            _schemaFactory.Setup(_ => _.Build("base-form"))
                .ReturnsAsync(formSchema);


            var viewModel = new Dictionary<string, object>();

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessMonthRequest(viewModel, form, path));
            Assert.Equal($"Requested path '{path}' object could not be found for form '{form}'", result.Message);
            _sessionHelper.Verify(_ => _.GetSessionGuid(), Times.Never);
        }

        [Fact]
        public async Task ProcessBooking_Should_Process_CheckYourBookingPage_And_ReserveBooking()
        {
            var appointmentTypeGuid = new Guid();
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = appointmentTypeGuid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .Build();

            var model = new Dictionary<string, object>{
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", DateTime.Now.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", DateTime.Now.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}", DateTime.Now.ToString() }
            };

            await _service.ProcessBooking(model, page, formSchema, "guid", "path");

            _bookingProvider.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            _mockMappingService.Verify(_ => _.MapBookingRequest(It.IsAny<string>(), It.IsAny<IElement>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ProcessBooking_Should_Return_CurrentPageEntity_When_CheckYourBooking_Property_IsFalse_AndReserve_Appointment()
        {
            var bookingInfo = new BookingInformation();

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object> { { $"bookingQuestion{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}", bookingInfo } } }));

            var appointmentTypeGuid = new Guid();
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = appointmentTypeGuid, Environment = "test" })
                .WithCheckYourBooking(false)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .Build();

            var model = new Dictionary<string, object>{
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}", DateTime.Now.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", DateTime.Now.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", DateTime.Now.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}", DateTime.Now.ToString() },
            };

            var result = await _service.ProcessBooking(model, page, formSchema, "guid", "path");

            _bookingProvider.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Never);
            Assert.IsType<ProcessRequestEntity>(result);
        }

        [Fact]
        public async Task ProcessBooking_Should_Return_CurrentPage_When_CheckYourBooking_IsTrue()
        {
            var bookingInfo = new BookingInformation();

            _mockDistributedCache.Setup(_ => _.GetString(It.Is<string>(_ => _.Equals("guid"))))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { FormData = new Dictionary<string, object> { { $"bookingQuestion{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}", bookingInfo } } }));

            var appointmentTypeGuid = new Guid();
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = appointmentTypeGuid, Environment = "test" })
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .Build();

            var model = new Dictionary<string, object>{
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_TYPE_SEARCH_RESULTS}", DateTime.Now.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", DateTime.Now.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", DateTime.Now.ToString() }
            };

            var result = await _service.ProcessBooking(model, page, formSchema, "guid", "path");

            _bookingProvider.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Never);
            _bookingProvider.Verify(_ => _.GetLocation(It.IsAny<LocationRequest>()), Times.Never);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Never);
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Never);
            Assert.IsType<ProcessRequestEntity>(result);
            Assert.Equal("Index", result.RedirectAction);
            Assert.True(result.RedirectToAction);
        }

        [Fact]
        public async Task ProcessBooking_Should_NotCall_BookingProvider_WhenReservedDateAndTime_IsSame_AsCurrentSelectedDateTime()
        {
            var appointmentTypeGuid = new Guid();
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = appointmentTypeGuid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .Build();

            var date = DateTime.Now;

            var model = new Dictionary<string, object>{
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_ID}", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_DATE}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_END_TIME}", date.ToString() },
            };

            await _service.ProcessBooking(model, page, formSchema, "guid", "path");

            _bookingProvider.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Never);
            _mockMappingService.Verify(_ => _.MapBookingRequest(It.IsAny<string>(), It.IsAny<IElement>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ProcessBooking_Should_Call_BookingProvider_When_SelectedDate_IsDifferent_To_ReservedDateAndTime()
        {
            
            var appointmentTypeGuid = new Guid();
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = appointmentTypeGuid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .Build();

            var date = DateTime.Now;

            var model = new Dictionary<string, object>{
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", date.AddDays(1).ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}",date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_ID}", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_DATE}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_END_TIME}", date.ToString() },
            };

            

            await _service.ProcessBooking(model, page, formSchema, "guid", "path");

            _bookingProvider.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            _mockMappingService.Verify(_ => _.MapBookingRequest(It.IsAny<string>(), It.IsAny<IElement>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }


        [Fact]
        public async Task ProcessBooking_Should_Call_BookingProvider_When_SelectedStartTime_IsDifferent_To_ReservedDateAndTime()
        {
            var appointmentTypeGuid = new Guid();
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = appointmentTypeGuid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .Build();

            var date = DateTime.Now;

            var model = new Dictionary<string, object>{
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", date.AddHours(1).ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_ID}", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_DATE}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_END_TIME}", date.ToString() }
            };

            await _service.ProcessBooking(model, page, formSchema, "guid", "path");

            _bookingProvider.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            _mockMappingService.Verify(_ => _.MapBookingRequest(It.IsAny<string>(), It.IsAny<IElement>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task ProcessBooking_Should_Call_BookingProvider_When_SelectedEndTime_IsDifferent_To_ReservedDateAndTime()
        {
            var appointmentTypeGuid = new Guid();
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType{ AppointmentId = appointmentTypeGuid, Environment = "test" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base-form")
                .Build();

            var date = DateTime.Now;

            var model = new Dictionary<string, object>{
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}", date.AddHours(1).ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_ID}", Guid.NewGuid().ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_DATE}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}", date.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_END_TIME}", date.ToString() },
            };

            await _service.ProcessBooking(model, page, formSchema, "guid", "path");

            _bookingProvider.Verify(_ => _.Reserve(It.IsAny<BookingRequest>()), Times.Once);
            _mockMappingService.Verify(_ => _.MapBookingRequest(It.IsAny<string>(), It.IsAny<IElement>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>()), Times.Once);
            _mockPageHelper.Verify(_ => _.SaveAnswers(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<CustomFormFile>>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
