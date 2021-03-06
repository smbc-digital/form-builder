﻿using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Validators;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class NumericValueElementValidatorTests
    {
        private readonly NumericValueValidator _validator = new NumericValueValidator();

        [Fact]
        public void Validate_ShouldReturnTrueValidationResult_WhenElementNotNumeric()
        {
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(false)
                .Build();

            var result = _validator.Validate(element, null, new form_builder.Models.FormSchema());

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrueValidationResult_WhenElementValidNumber()
        {
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "123");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.True(result.IsValid);
            Assert.Equal("", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalseValidationResult_WhenElementNotValidNumber()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "a123");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be a whole number", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnCustomValidationResult_WhenElementNotValidNumber()
        {
            var errorMessage = "Provide an integer";
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .Build();

            element.Properties.NotAnIntegerValidationMessage = errorMessage;

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "a123");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal(errorMessage, result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalseValidationResult_WhenValueGreaterThanMax()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMax("20")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "123");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be less than or equal to 20", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalseValidationResult_WhenValueLessThanMin()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMin("20")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "12");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be greater than or equal to 20", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalseValidationResult_WhenOutOfRange()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("tets-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMax("20")
                .WithMin("10")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("tets-id", "7");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be between 10 and 20 inclusive", result.Message);
        }

        [Fact]
        public void Validate_ShouldReturnFalseValidationResult_WhenGreaterThanMaxLength()
        {
            var label = "Test label";
            var element = new ElementBuilder()
                .WithQuestionId("test-id")
                .WithNumeric(true)
                .WithLabel(label)
                .WithMaxLength(7)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-id", "12345678");

            var result = _validator.Validate(element, viewModel, new form_builder.Models.FormSchema());

            Assert.False(result.IsValid);
            Assert.Equal($"{label} must be 7 digits or less", result.Message);
        }
    }
}
