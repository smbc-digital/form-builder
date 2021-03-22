using form_builder.Builders;
using form_builder.Enum;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class QuestionIdExistsForBookingCustomerAddressIdCheckTests
    {
        
        [Fact]
        public void QuestionIdExistsForBookingCustomerAddressIdCheck_IsNotValid_WhenQuestionIdDoesNotExist()
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

            var page = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();
            
            var page3 = new PageBuilder()
                .WithElement(additionalBookingElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .WithPage(page3)
                .WithName("test-name")
                .Build();

            var check = new QuestionIdExistsForBookingCustomerAddressIdCheck();

            // Act
            var result = check.Validate(schema);
            
            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CheckQuestionIdExistsForBookingCustomerAddressId_ShouldNotThrowException_WhenQuestionIdExists()
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

            var page = new PageBuilder()
                .WithElement(addressElement)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(bookingElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .WithName("test-name")
                .Build();

            var check = new QuestionIdExistsForBookingCustomerAddressIdCheck();

            // Act
            var result = check.Validate(schema);
            
            // Assert
            Assert.True(result.IsValid);
        }
    }
}