using System;
using System.Collections.Generic;
using form_builder.Conditions;
using form_builder.Enum;
using form_builder.Models;
using Xunit;

namespace form_builder_tests.UnitTests.Conditions
{
    public class ConditionTests
    {
        [Fact]
        public void Equal_To_Should_Return_True_False()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", "pear"},
                {"test2", "apple"}
            };

            var condition1 = new Condition { EqualTo = "pear", QuestionId = "test", ConditionType = ECondition.EqualTo };
            var condition2 = new Condition { EqualTo = "plum", QuestionId = "test2", ConditionType = ECondition.EqualTo };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
            Assert.False(conditionValidator.IsValid(condition2, viewModel));
        }

        [Fact]
        public void CheckBox_Contains_Should_Return_True_False()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", "pear,guava"},
                {"test2", "apple,mango"}
            };

            var condition1 = new Condition { CheckboxContains = "pear", QuestionId = "test", ConditionType = ECondition.Contains };
            var condition2 = new Condition { CheckboxContains = "plum", QuestionId = "test2", ConditionType = ECondition.Contains };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
            Assert.False(conditionValidator.IsValid(condition2, viewModel));
        }

        [Fact]
        public void Is_Null_Or_Empty_Should_Return_True_False()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test", ""},
                {"test2", "apple"}
            };

            var condition1 = new Condition { IsNullOrEmpty = true, QuestionId = "test", ConditionType = ECondition.IsNullOrEmpty };
            var condition2 = new Condition { IsNullOrEmpty = false, QuestionId = "test2", ConditionType = ECondition.IsNullOrEmpty };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
            Assert.True(conditionValidator.IsValid(condition2, viewModel));
        }

        [Fact]
        public void Is_Before_Should_Return_True()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(4);
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-day", futureDate.Day.ToString()},
                {"test-month", futureDate.Month.ToString()},
                {"test-year", futureDate.Year.ToString()}
            };

            var condition1 = new Condition { IsBefore = 10, Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsBefore };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void Is_Before_Should_Return_False()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(20);
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-day", futureDate.Day.ToString()},
                {"test-month", futureDate.Month.ToString()},
                {"test-year", futureDate.Year.ToString()}
            };

            var condition1 = new Condition { IsBefore = 10, Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsBefore };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.False(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void Is_After_Should_Return_False()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(2);
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-day", futureDate.Day.ToString()},
                {"test-month", futureDate.Month.ToString()},
                {"test-year", futureDate.Year.ToString()}
            };

            var condition1 = new Condition { IsAfter = 10, Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsAfter };
            var conditionValidator = new ConditionValidator();
            
            // Act & Assert
            Assert.False(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void Is_After_Should_Return_True()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(20);
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-day", futureDate.Day.ToString()},
                {"test-month", futureDate.Month.ToString()},
                {"test-year", futureDate.Year.ToString()}
            };

            var condition1 = new Condition { IsAfter = 10, Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsAfter };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
        }
    }
}