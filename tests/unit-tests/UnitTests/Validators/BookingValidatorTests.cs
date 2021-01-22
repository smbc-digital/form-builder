using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class BookingValidatorTests
    {
        private readonly BookingValidator _validator = new BookingValidator();

        [Fact]
        public void Validate_ShouldReturnTrue_When_Element_IsNotBookingElement()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_When_Element_IsBooking_AndOnCheck_YourBookingPage()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { LookUpConstants.SubPathViewModelKey, BookingConstants.CHECK_YOUR_BOOKING }
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_When_ViewModel_DoesNotContainValue_AndIsOptional()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .WithOptional(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_ViewModel_DoesNotContainValue()
        {
             // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.BOOKING_DATE_EMPTY, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_DateTime_IsInvalid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", "not-a-date" }
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.BOOKING_DATE_EMPTY, result.Message);
        }
        
        
        [Fact]
        public void Validate_ShouldReturn_AndUseCustomer_ErrorMessage()
        {
            // Arrange
            var customeMessage = "customer validation message";
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .WithCustomValidationMessage(customeMessage)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(customeMessage, result.Message);
        }
        

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDateIsValid_AndRequired()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", DateTime.Today.ToString() }
            };

            // Act
            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
