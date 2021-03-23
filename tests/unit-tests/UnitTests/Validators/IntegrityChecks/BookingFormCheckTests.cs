using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
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

        [Theory]
        [InlineData("customer.firstname", "customer.lastname", BookingConstants.NO_APPOINTMENT_AVAILABLE)]
        public void BookingFormCheck_IsValid(string firstName, string lastName,string pageSlug)
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
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}     
