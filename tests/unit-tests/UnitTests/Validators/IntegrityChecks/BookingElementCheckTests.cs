using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Models.Booking.Request;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class BookingCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        
        public BookingCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public void BookingElementCheck_IsNotValid_WhenForm_DoesNotContain_RequiredCustomerFields()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("Fake")
                .WithAppointmentType(new AppointmentType { AppointmentId = Guid.NewGuid(), Environment = "local" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var appointmentPage = new PageBuilder()
                .WithPageSlug(BookingConstants.NO_APPOINTMENT_AVAILABLE)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(appointmentPage)
                .Build();

            var check = new BookingElementCheck(_mockHostingEnv.Object);

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Equal(1, result.Messages.Count);
        }

        [Fact]
        public void BookingElementCheck_IsNotValid_WhenForm_DoesNotContain_Required_NoAppointmentsPage()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("Fake")
                .WithAppointmentType(new AppointmentType{ AppointmentId = Guid.NewGuid(), Environment = "local" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            var check = new BookingElementCheck(_mockHostingEnv.Object);

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Booking Element Check, Form contains booking element, but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE} on form test-name.")));
        }

        [Fact]
        public void BookingElementCheck_IsNotValid_WhenBookingElement_DoesNotContains_AppointmentType()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("Fake")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            var check = new BookingElementCheck(_mockHostingEnv.Object);

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Booking Element Check, Booking element 'booking' requires a AppointmentType property on form test-name.")));
            //Assert.Equal("PageHelper:CheckForBookingElement, Booking element requires a AppointmentType property.", result.Message);
        }

        [Fact]
        public void BookingElementCheck_IsNotValid_WhenBookingElement_DoesNotContains_BookingProvider()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithAppointmentType(new AppointmentType{ AppointmentId = Guid.NewGuid(), Environment = "local" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            var check = new BookingElementCheck(_mockHostingEnv.Object);

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Booking Element Check, Booking element 'booking' requires a valid booking provider property on form test-name.")));
        }

        [Fact]
        public void BookingElementCheck_IsNotValid_WhenBookingElement_Contains_EmptyGuid_ForOptionalResources()
        {
            // Arrange
            var pages = new List<Page>();

            var appointmentType = new AppointmentTypeBuilder()
                .WithEnvironment("local")
                .WithOptionalResource(new BookingResource { ResourceId = Guid.Empty, Quantity = 1 })
                .Build();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("TestProvider")
                .WithAppointmentType(appointmentType)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page)
                .Build();

            var check = new BookingElementCheck(_mockHostingEnv.Object);

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Booking Element Check, Booking element 'booking', optional resources are invalid, ResourceId cannot be an empty Guid on form test-name.")));        
        }  
    }
}     
