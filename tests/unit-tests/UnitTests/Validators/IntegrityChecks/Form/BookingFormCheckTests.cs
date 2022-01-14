using System;
using System.Linq;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class BookingFormCheckTests
    {
        [Theory]
        [InlineData("AppointmentI", "AppointmentId", "", "Real_Provider", "customer.firstname", "customer.lastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        [InlineData("", "", "022ebc92-1c51-4a68-a079-f6edefc63a07", "Any_Provider", "customer.firstname", "customer.lastname", "test-page")]
        [InlineData("", "", "022ebc92-1c51-4a68-a079-f6edefc63a07", "Any_Provider", "customer.firstname", "customerlastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        [InlineData("", "", "022ebc92-1c51-4a68-a079-f6edefc63a07", "Any_Provider", "customerfirstname", "customer.lastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        public void BookingFormCheck_IsNotValid_WillFailOnOneAspect(
            string questionId,
            string appointmentIdKey,
            string appointmentIdGuid,
            string bookingProvider,
            string firstName,
            string lastName,
            string pageSlug)
        {
            // Arrange
            var realAppointmentId = "022ebc92-1c51-4a68-a079-f6edefc63a07";
            var selectAppointmentType = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId(questionId)
                .WithOptions(new()
                {
                    new()
                    {
                        Text = "Appointment Type",
                        Value = realAppointmentId
                    }
                })
                .Build();

            var customerFirstName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping(firstName)
                .Build();

            var customerLastName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping(lastName)
                .Build();

            var page1 = new PageBuilder()
                .WithElement(selectAppointmentType)
                .WithElement(customerFirstName)
                .WithElement(customerLastName)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider(bookingProvider)
                .WithAppointmentType(new()
                {
                    Environment = "any",
                    AppointmentId = string.IsNullOrEmpty(appointmentIdGuid) ? new Guid() : new Guid(appointmentIdGuid),
                    AppointmentIdKey = appointmentIdKey
                })
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug(pageSlug)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            BookingFormCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages.Where(message => message.StartsWith(IntegrityChecksConstants.FAILURE)));
        }

        [Theory]
        [InlineData("", "", "", BookingConstants.FAKE_PROVIDER, "customer.firstname", "customer.lastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        [InlineData("AppointmentId", "AppointmentId", "", "Real_Provider", "customer.firstname", "customer.lastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        [InlineData("", "", "022ebc92-1c51-4a68-a079-f6edefc63a07", "Any_Provider", "customer.firstname", "customer.lastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        public void BookingFormCheck_IsValid(
            string questionId,
            string appointmentIdKey,
            string appointmentIdGuid,
            string bookingProvider,
            string firstName,
            string lastName,
            string pageSlug)
        {
            // Arrange
            var realAppointmentId = "022ebc92-1c51-4a68-a079-f6edefc63a07";
            var selectAppointmentType = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId(questionId)
                .WithOptions(new()
                {
                    new()
                    {
                        Text = "Appointment Type",
                        Value = realAppointmentId
                    }
                })
                .Build();

            var customerFirstName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping(firstName)
                .Build();

            var customerLastName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping(lastName)
                .Build();

            var page1 = new PageBuilder()
                .WithElement(selectAppointmentType)
                .WithElement(customerFirstName)
                .WithElement(customerLastName)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider(bookingProvider)
                .WithAppointmentType(new()
                {
                    Environment = "any",
                    AppointmentId = string.IsNullOrEmpty(appointmentIdGuid) ? new Guid() : new Guid(appointmentIdGuid),
                    AppointmentIdKey = appointmentIdKey
                })
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug(pageSlug)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            BookingFormCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Messages.Where(message => message.StartsWith(IntegrityChecksConstants.FAILURE)));
        }

        [Fact]
        public void BookingFormCheck_IsValid_If_FormSchema_Contains_Multiple_BookingElements_But_Same_Provider()
        {
            // Arrange
            var customerFirstName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping("customer.firstname")
                .Build();

            var customerLastName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping("customer.lastname")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(customerFirstName)
                .WithElement(customerLastName)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testprovider")
                .Build();

            // Arrange
            var bookingElement2 = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testprovider")
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug(BookingConstants.NO_APPOINTMENT_AVAILABLE)
                .WithElement(bookingElement2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            BookingFormCheck check = new();
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Messages.Where(message => message.StartsWith(IntegrityChecksConstants.FAILURE)));
        }

        [Fact]
        public void BookingFormCheck_Should_Return_Failure_Message_If_FormSchema_Contains_Multiple_BookingProviders()
        {
            // Arrange
            var customerFirstName = new ElementBuilder()
               .WithType(EElementType.Textbox)
               .WithTargetMapping("customer.firstname")
               .Build();

            var customerLastName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping("customer.lastname")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(customerFirstName)
                .WithElement(customerLastName)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testprovider")
                .Build();

            // Arrange
            var bookingElement2 = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("differentProvider")
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .WithElement(bookingElement2)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug(BookingConstants.NO_APPOINTMENT_AVAILABLE)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            BookingFormCheck check = new();
            var result = check.Validate(schema);
            var failureMessages = result.Messages.Where(message => message.StartsWith(IntegrityChecksConstants.FAILURE));

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(failureMessages);
            Assert.Contains(BookingConstants.INTEGRITY_FAILURE_MESSAGE_DUPLICATEPROVIDER, failureMessages.First());
        }

        [Fact]
        public void BookingFormCheck_Should_Return_Failure_Message_If_FormSchema_Contains_Multiple_BookingProviders_OnDifferentPages()
        {
            // Arrange
            var customerFirstName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping("customer.firstname")
                .Build();

            var customerLastName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping("customer.lastname")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(customerFirstName)
                .WithElement(customerLastName)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("testprovider")
                .Build();

            // Arrange
            var bookingElement2 = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithBookingProvider("differentProvider")
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug(BookingConstants.NO_APPOINTMENT_AVAILABLE)
                .WithElement(bookingElement2)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("test-name")
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            BookingFormCheck check = new();
            var result = check.Validate(schema);
            var failureMessages = result.Messages.Where(message => message.StartsWith(IntegrityChecksConstants.FAILURE));

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(failureMessages);
            Assert.Contains(BookingConstants.INTEGRITY_FAILURE_MESSAGE_DUPLICATEPROVIDER, failureMessages.First());
        }
    }
}
