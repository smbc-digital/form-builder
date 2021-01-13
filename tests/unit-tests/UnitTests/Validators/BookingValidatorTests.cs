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
            var result = _validator.Validate(element, viewModel);

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
            var result = _validator.Validate(element, viewModel);

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
            var result = _validator.Validate(element, viewModel);

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

            var viewModel = new Dictionary<string, dynamic>
            {
                {$"question-{BookingConstants.APPOINTMENT_START_TIME}", "01/01/2000"}
            };

            // Act
            var result = _validator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.BOOKING_DATE_EMPTY, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_Date_IsInvalid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", "not-a-date" },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", "01/01/2000T13:00:00" }
            };

            // Act
            var result = _validator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.BOOKING_DATE_EMPTY, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_Time_IsInvalid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", "01/01/2000" },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", "not a time" }
            };

            // Act
            var result = _validator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.BOOKING_TIME_EMPTY, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_Time_Is_NotSupplied()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", "01/01/2000" }
            };

            // Act
            var result = _validator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationConstants.BOOKING_TIME_EMPTY, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturn_And_UseCustom_ErrorMessage()
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
            var result = _validator.Validate(element, viewModel);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(customeMessage, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenDate_AndTime_IsValid_AndRequired()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("question")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}", DateTime.Today.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}", DateTime.Today.ToString() },
                { $"{element.Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}", DateTime.Today.ToString() }
            };

            // Act
            var result = _validator.Validate(element, viewModel);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
