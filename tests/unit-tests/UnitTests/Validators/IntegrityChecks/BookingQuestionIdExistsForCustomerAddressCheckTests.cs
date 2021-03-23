using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class BookingQuestionIdExistsForCustomerAddressCheckTests
    {
        
        [Fact]
        public void BookingQuestionIdExistsForCustomerAddressCheck_NotValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithQuestionId("address")
                .WithType(EElementType.Textarea)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithQuestionId("booking")
                .WithType(EElementType.Booking)
                .WithCustomerAddressId("address")
                .Build();

            var additionalBookingElement = new ElementBuilder()
                .WithQuestionId("additionalBooking")
                .WithType(EElementType.Booking)
                .WithCustomerAddressId("additionalAddress")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();
            
            var page3 = new PageBuilder()
                .WithElement(additionalBookingElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithPage(page3)
                .WithName("test-name")
                .Build();


            // Act
            BookingQuestionIdExistsForCustomerAddressCheck check = new();
            var result = check.Validate(schema);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void BookingQuestionIdExistsForCustomerAddressCheck_Valid()
        {
            // Arrange
            var addressElement = new ElementBuilder()
                .WithQuestionId("address")
                .WithType(EElementType.Textarea)
                .Build();

            var bookingElement = new ElementBuilder()
                .WithQuestionId("booking")
                .WithType(EElementType.Booking)
                .WithCustomerAddressId("address")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(addressElement)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page1)
                .WithPage(page2)
                .WithName("test-name")
                .Build();


            // Act
            BookingQuestionIdExistsForCustomerAddressCheck check = new();
            var result = check.Validate(schema);
            
            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}