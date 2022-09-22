using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Booking;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class BookingTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        public BookingTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task RenderAsync_ShouldCallViewRenderWithCorrectPartial_When_CalendarView()
        {
            //Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Booking)
               .WithBookingProvider("testBookingProvider")
               .WithQuestionId("bookingQuestion")
               .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
               .WithCheckYourBooking(true)
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>()
                }
            };

            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Booking"), It.IsAny<Booking>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldCallViewRenderWithCorrectPartial_When_CheckYourBooking()
        {
            //Arrange
            var key = $"bookingQuestion-{BookingConstants.APPOINTMENT_DATE}";
            var keyStart = $"bookingQuestion-{BookingConstants.APPOINTMENT_START_TIME}";
            var keyEnd = $"bookingQuestion-{BookingConstants.APPOINTMENT_END_TIME}";

            _mockElementHelper
                .Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(key)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            _mockElementHelper
                .Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(keyStart)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            _mockElementHelper
                .Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(keyEnd)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING },
                { key, DateTime.Today.ToString() },
                { keyStart, DateTime.Today.ToString() },
                { keyEnd, DateTime.Today.ToString() },
            };

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>
                    {
                        new AvailabilityDayResponse {
                            Date = DateTime.Today
                        }
                    }
                }
            };

            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "CheckYourBooking"), It.IsAny<Booking>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }


        [Fact]
        public async Task RenderAsync_Should_GenerateCorrectModel_Properties_ForCalendar_Page()
        {
            //Arrange
            var baseUrl = "form-base";
            var pageUrl = "test-page-url";
            var key = $"bookingQuestion-{BookingConstants.APPOINTMENT_DATE}";

            _mockElementHelper
                .Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(key)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            var element = new ElementBuilder()
               .WithType(EElementType.Booking)
               .WithBookingProvider("testBookingProvider")
               .WithQuestionId("bookingQuestion")
               .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
               .WithCheckYourBooking(true)
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug(pageUrl)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form Name")
                .WithBaseUrl(baseUrl)
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>
                    {
                        new AvailabilityDayResponse {
                            Date = DateTime.Today,
                            AppointmentTimes = new List<AppointmentTime> {
                                new AppointmentTime {
                                    EndTime = new TimeSpan(6, 0, 0),
                                    StartTime = new TimeSpan(17, 0, 0)
                                }
                            }
                        }
                    },
                    CurrentSearchedMonth = DateTime.Today,
                    FirstAvailableMonth = DateTime.Today.AddMonths(-1),
                    IsFullDayAppointment = false,
                    AppointmentStartTime= DateTime.Today.AddHours(-1),
                    AppointmentEndTime = DateTime.Today.AddHours(1)
                }
            };

            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            var bookingElement = Assert.IsType<Booking>(element);
            Assert.Equal($"/booking/{baseUrl}/{pageUrl}/month", bookingElement.MonthSelectionPostUrl);
            Assert.NotEmpty(bookingElement.Calendar);
            Assert.NotEmpty(bookingElement.Appointments);
            Assert.Equal("form Name", bookingElement.FormName);
            Assert.Equal(DateTime.Today.AddMonths(-1), bookingElement.FirstAvailableMonth);
            Assert.Equal(DateTime.Today, bookingElement.CurrentSelectedMonth);
            Assert.False(bookingElement.IsAppointmentTypeFullDay);
            Assert.False(bookingElement.DisplayInsetText);
            Assert.Equal(string.Empty, bookingElement.InsetText);
        }

        [Fact]
        public async Task RenderAsync_Should_GenerateCorrectModel_Properties_ForCheckYourBooking_Page()
        {
            //Arrange
            var key = $"bookingQuestion-{BookingConstants.APPOINTMENT_DATE}";
            _mockElementHelper.Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(key)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING },
                { key, DateTime.Today.ToString() }
            };

            var schema = new FormSchemaBuilder()
                .WithName("form Name")
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>
                    {
                        new AvailabilityDayResponse {
                            Date = DateTime.Today,
                            AppointmentTimes = new List<AppointmentTime> {
                                new AppointmentTime {
                                    EndTime = new TimeSpan(6, 0, 0),
                                    StartTime = new TimeSpan(17, 0, 0)
                                }
                            }
                        }
                    },
                    CurrentSearchedMonth = DateTime.Today,
                    FirstAvailableMonth = DateTime.Today.AddMonths(-1),
                    IsFullDayAppointment = true,
                    AppointmentStartTime= DateTime.Today.AddHours(-1),
                    AppointmentEndTime = DateTime.Today.AddHours(1)
                }
            };

            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            var bookingElement = Assert.IsType<Booking>(element);
            Assert.NotNull(bookingElement.SelectedBooking);
            Assert.NotEmpty(bookingElement.Appointments);
            Assert.Equal("form Name", bookingElement.FormName);
            Assert.Equal(DateTime.Today.AddMonths(-1), bookingElement.FirstAvailableMonth);
            Assert.Equal(DateTime.Today, bookingElement.CurrentSelectedMonth);
            Assert.Equal(DateTime.Today.AddHours(-1), bookingElement.AppointmentStartTime);
            Assert.Equal(DateTime.Today.AddHours(1), bookingElement.AppointmentEndTime);
            Assert.True(bookingElement.IsAppointmentTypeFullDay);
            Assert.True(bookingElement.DisplayInsetText);
            Assert.Equal("You can select a date but you cannot select a time. We’ll be with you between 11pm and 1am.", bookingElement.InsetText);
        }

        [Fact]
        public async Task RenderAsync_Should_SelectFirstAvailableDay_WhenOnFirstAvailableMonth_AndNoDateCurrentlySelected()
        {
            //Arrange
            var date = DateTime.Now;
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>
                    {
                        new AvailabilityDayResponse
                        {
                            AppointmentTimes = new List<AppointmentTime>
                            {
                                new AppointmentTime
                                {
                                    StartTime = new TimeSpan(5, 0, 0),
                                    EndTime= new TimeSpan(17, 0, 0),
                                }
                            },
                            Date = date
                        }
                    }
                }
            };
            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            var bookingElement = Assert.IsType<Booking>(element);
            Assert.NotNull(bookingElement.Properties.Value);
            Assert.Equal(date.ToString(), bookingElement.Properties.Value);
        }

        [Fact]
        public async Task RenderAsync_Should_Not_SelectFirstAvailableDay_WhenValue_IsNull_AndNotOn_FirstAvailableMonth()
        {
            //Arrange
            var date = DateTime.Now;
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>
                    {
                        new AvailabilityDayResponse
                        {
                            AppointmentTimes = new List<AppointmentTime>
                            {
                                new AppointmentTime
                                {
                                    StartTime = new TimeSpan(5, 0, 0),
                                    EndTime= new TimeSpan(17, 0, 0),
                                }
                            },
                            Date = date
                        }
                    },
                    CurrentSearchedMonth = DateTime.Now.AddMonths(1),
                    IsFullDayAppointment = true
                }
            };
            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            var bookingElement = Assert.IsType<Booking>(element);
            Assert.Null(bookingElement.Properties.Value);
        }


        [Fact]
        public async Task RenderAsync_Should_Create_Times_When_On_CalendarJourney_WhenAppointmentIsNot_FullDay()
        {
            //Arrange
            var date = DateTime.Now;
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
                .WithCheckYourBooking(true)
                .WithNoAvailableTimeForBookingType("test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>
                    {
                        new AvailabilityDayResponse
                        {
                            AppointmentTimes = new List<AppointmentTime>
                            {
                                new AppointmentTime
                                {
                                    StartTime = new TimeSpan(13, 0, 0),
                                    EndTime= new TimeSpan(14, 0, 0),
                                },
                                new AppointmentTime
                                {
                                    StartTime = new TimeSpan(3, 0, 0),
                                    EndTime= new TimeSpan(4, 0, 0),
                                },
                                new AppointmentTime
                                {
                                    StartTime = new TimeSpan(9, 0, 0),
                                    EndTime= new TimeSpan(10, 0, 0),
                                }
                            },
                            Date = date
                        }
                    }
                }
            };
            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            var bookingElement = Assert.IsType<Booking>(element);
            Assert.Single(bookingElement.Times);
            Assert.Equal(2, bookingElement.Times.First().MorningAppointments.Appointments.Count);
            Assert.Single(bookingElement.Times.First().AfternoonAppointments.Appointments);
            Assert.Equal(ETimePeriod.Morning, bookingElement.Times.First().TimePeriodCurrentlySelected);
            Assert.Equal("test", bookingElement.Times.First().MorningAppointments.BookingType);
        }

        [Fact]
        public async Task RenderAsync_Should_Select_Afternoon_WhenNoMorningAppointments_AsTimePeriod_CurrentlySelected()
        {
            //Arrange
            var date = DateTime.Now;
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "test" })
                .WithCheckYourBooking(true)
                .WithNoAvailableTimeForBookingType("test2")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookingInfo = new List<object>
            {
                new BookingInformation
                {
                    Appointments = new List<AvailabilityDayResponse>
                    {
                        new AvailabilityDayResponse
                        {
                            AppointmentTimes = new List<AppointmentTime>
                            {
                                new AppointmentTime
                                {
                                    StartTime = new TimeSpan(13, 0, 0),
                                    EndTime= new TimeSpan(14, 0, 0),
                                }
                            },
                            Date = date
                        }
                    }
                }
            };
            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookingInfo);

            //Assert
            var bookingElement = Assert.IsType<Booking>(element);
            Assert.Single(bookingElement.Times);
            Assert.Empty(bookingElement.Times.First().MorningAppointments.Appointments);
            Assert.Single(bookingElement.Times.First().AfternoonAppointments.Appointments);
            Assert.Equal(ETimePeriod.Afternoon, bookingElement.Times.First().TimePeriodCurrentlySelected);
            Assert.Equal("test2", bookingElement.Times.First().MorningAppointments.BookingType);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_IsDefined()
        {
            var label = "Custom label";
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking-test")
                .WithSummaryLabel(label)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal(label, result);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_Is_Not_Defined()
        {
            var pageTitle = "Booking";
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking-test")
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal(pageTitle, result);
        }
    }
}