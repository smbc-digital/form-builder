using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Booking;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Models.Booking.Response;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class BookingTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

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
                .WithAppointmentType(Guid.NewGuid())
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookignInfo = new List<object>
            {
                new BookingInformation{
                    Appointments = new List<AvailabilityDayResponse>()
                }
            };
            var formAnswers = new FormAnswers();
            //Act
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                "",
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers, 
                bookignInfo);

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
            _mockElementHelper.Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(key)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            _mockElementHelper.Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(keyStart)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            _mockElementHelper.Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(keyEnd)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(Guid.NewGuid())
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

            var bookignInfo = new List<object>
            {
                new BookingInformation{
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
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                "",
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers, 
                bookignInfo);

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
            _mockElementHelper.Setup(_ => _.CurrentValue(It.Is<string>(_ => _.Equals(key)), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(DateTime.Today.ToString());

             var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testBookingProvider")
                .WithQuestionId("bookingQuestion")
                .WithAppointmentType(Guid.NewGuid())
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

            var bookignInfo = new List<object>
            {
                new BookingInformation{
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
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                "",
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers, 
                bookignInfo);

            //Assert
            var bookignElement = Assert.IsType<Booking>(element);
            Assert.Equal($"/booking/{baseUrl}/{pageUrl}/month", bookignElement.MonthSelectionPostUrl);
            Assert.NotEmpty(bookignElement.Calendar);
            Assert.NotEmpty(bookignElement.Appointments);
            Assert.Equal("form Name", bookignElement.FormName);
            Assert.Equal(DateTime.Today.AddMonths(-1), bookignElement.FirstAvailableMonth);
            Assert.Equal(DateTime.Today, bookignElement.CurrentSelectedMonth);
            Assert.False(bookignElement.IsAppointmentTypeFullDay);
            Assert.False(bookignElement.DisplayInsetText);
            Assert.Equal(string.Empty, bookignElement.InsetText);
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
                .WithAppointmentType(Guid.NewGuid())
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

            var bookignInfo = new List<object>
            {
                new BookingInformation{
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
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                "",
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers, 
                bookignInfo);

             //Assert
            var bookignElement = Assert.IsType<Booking>(element);
            Assert.NotNull(bookignElement.SelectedBooking);
            Assert.NotEmpty(bookignElement.Appointments);
            Assert.Equal("form Name", bookignElement.FormName);
            Assert.Equal(DateTime.Today.AddMonths(-1), bookignElement.FirstAvailableMonth);
            Assert.Equal(DateTime.Today, bookignElement.CurrentSelectedMonth);
            Assert.Equal(DateTime.Today.AddHours(-1), bookignElement.AppointmentStartTime);
            Assert.Equal(DateTime.Today.AddHours(1), bookignElement.AppointmentEndTime);
            Assert.True(bookignElement.IsAppointmentTypeFullDay);
            Assert.True(bookignElement.DisplayInsetText);
            Assert.Equal("You can select a date for form Name but you can not select a time. We'll be with you between 11pm and 1am.", bookignElement.InsetText);
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
                .WithAppointmentType(Guid.NewGuid())
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookignInfo = new List<object>
            {
                new BookingInformation{
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
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers, 
                bookignInfo);

            //Assert
            var bookignElement = Assert.IsType<Booking>(element);
            Assert.NotNull(bookignElement.Properties.Value);
            Assert.Equal(date.ToString(), bookignElement.Properties.Value);
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
                .WithAppointmentType(Guid.NewGuid())
                .WithCheckYourBooking(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var bookignInfo = new List<object>
            {
                new BookingInformation{
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
                    CurrentSearchedMonth = DateTime.Now.AddMonths(1)
                }
            };
            var formAnswers = new FormAnswers();
            //Act
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                bookignInfo);

            //Assert
            var bookignElement = Assert.IsType<Booking>(element);
            Assert.Null(bookignElement.Properties.Value);
        }
    }
}