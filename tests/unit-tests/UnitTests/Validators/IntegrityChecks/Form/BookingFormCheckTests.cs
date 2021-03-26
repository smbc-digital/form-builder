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
        [InlineData("customer.firstname", "customer.lastname", "test-page")]
        [InlineData("customer.firstname", "customerlastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        [InlineData("customerfirstname", "customer.lastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        public void BookingFormCheck_IsNotValid(string firstName, string lastName,string pageSlug)
        {
            // Arrange
            var customerFirstName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping(firstName)
                .Build();

            var customerLastName = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping(lastName)
                .Build();

            var page1 = new PageBuilder()
                .WithElement(customerFirstName)
                .WithElement(customerLastName)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithType(EElementType.Booking)
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
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void BookingFormCheck_IsValid()
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
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
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

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
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

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}Booking Element Check, Form contains different booking provider. Only one provider allows on for form", result.Messages.First());
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

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Messages);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}Booking Element Check, Form contains different booking provider. Only one provider allows on for form", result.Messages.First());
        }
    }
}     
