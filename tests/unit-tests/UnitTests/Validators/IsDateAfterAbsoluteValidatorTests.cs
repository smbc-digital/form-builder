﻿using System;
using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class IsDateAfterAbsoluteValidatorTests
    {
        private readonly IsDateAfterAbsoluteValidator _isDateAfterAbsoluteValidator = new IsDateAfterAbsoluteValidator();

        [Fact]
        public void Validate_ShouldReturnValidIfElemenTypeIsNotDateInputOrDatePicker()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            // Act
            ValidationResult result = _isDateAfterAbsoluteValidator.Validate(element, null, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }
        [Fact]
        public void Validate_ShouldReturnValidWhenFieldsAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test-date")
                .WithLabel("Date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            ValidationResult result = _isDateAfterAbsoluteValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnValidWhenDatePickerIsValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithIsDateAfterAbsolute("01/01/2020")
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date", "01/01/2021");

            // Act
            ValidationResult result = _isDateAfterAbsoluteValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnValidWhenDateInputIsValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithIsDateAfterAbsolute("01/01/2020")
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date-day", "01");
            viewModel.Add("test-date-month", "01");
            viewModel.Add("test-date-year", "2021");

            // Act
            ValidationResult result = _isDateAfterAbsoluteValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnNotValidWhenDatePickerIsNotValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithIsDateAfterAbsolute("01/01/2020")
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date", "01/01/2019");

            // Act
            ValidationResult result = _isDateAfterAbsoluteValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnNotValidWhenDatePickerIsOnBoundaryCondition()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithIsDateAfterAbsolute("01/01/2020")
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date", "01/01/2020");

            // Act
            ValidationResult result = _isDateAfterAbsoluteValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnNotValidWhenDateInputIsNotValid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithIsDateAfterAbsolute("01/01/2020")
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date-day", "01");
            viewModel.Add("test-date-month", "01");
            viewModel.Add("test-date-year", "2019");

            // Act
            ValidationResult result = _isDateAfterAbsoluteValidator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_Should_ThrowException_If_ComparisondDate_IsNotAValidDate()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithIsDateAfterAbsolute("NotAValidDate")
                .WithQuestionId("test-date")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-date", "01/01/2020");

            // Act/Asset
            Assert.Throws<FormatException>(() => _isDateAfterAbsoluteValidator.Validate(element, viewModel, new form_builder.Models.FormSchema()));
        }
    }
}
