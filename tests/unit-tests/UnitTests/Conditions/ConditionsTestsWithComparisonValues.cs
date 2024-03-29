﻿using form_builder.Conditions;
using form_builder.Enum;
using form_builder.Models;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Conditions
{
    public class ConditionTestsWithComparisonValue
    {
        [Fact]
        public void Equal_To_Should_Return_True_False()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                { "test", "pear" },
                { "test2", "apple" }
            };

            var condition1 = new Condition { ComparisonValue = "pear", QuestionId = "test", ConditionType = ECondition.EqualTo };
            var condition2 = new Condition { ComparisonValue = "plum", QuestionId = "test2", ConditionType = ECondition.EqualTo };
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

            var condition1 = new Condition { ComparisonValue = "pear", QuestionId = "test", ConditionType = ECondition.Contains };
            var condition2 = new Condition { ComparisonValue = "plum", QuestionId = "test2", ConditionType = ECondition.Contains };
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
                {"test", string.Empty},
                {"test2", "apple,mango"}
            };

            var condition1 = new Condition { ComparisonValue = "true", QuestionId = "test", ConditionType = ECondition.IsNullOrEmpty };
            var condition2 = new Condition { ComparisonValue = "false", QuestionId = "test2", ConditionType = ECondition.IsNullOrEmpty };
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

            var condition1 = new Condition { ComparisonValue = "10", Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsBefore };
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

            var condition1 = new Condition { ComparisonValue = "10", Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsBefore };
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

            var condition1 = new Condition { ComparisonValue = "10", Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsAfter };
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

            var condition1 = new Condition { ComparisonValue = "10", Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsAfter };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void IsFileNullOrEmpty_Should_Return_True()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                { "test-fileupload", null }
            };

            var condition1 = new Condition { ComparisonValue = "true", QuestionId = "test", ConditionType = ECondition.IsFileUploadNullOrEmpty };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void IsFileNullOrEmpty_Should_Return_False()
        {
            // Arrange
            var file = new List<FileUploadModel>
            {
                new FileUploadModel
                {
                    Key = "file-File-fileupload-cf03afbd-4b3d-48f0-a803-b252441aa93f",
                    Content = null,
                    TrustedOriginalFileName = "All shook up cast.jpg",
                    UntrustedOriginalFileName = "All shook up cast.jpg",
                    FileSize = 88717,
                    FileName = null
                }
            };

            var viewModel = new Dictionary<string, dynamic>
            {
                { "test-fileupload", JsonConvert.SerializeObject(file) }
            };

            var condition1 = new Condition { ComparisonValue = "true", QuestionId = "test", ConditionType = ECondition.IsFileUploadNullOrEmpty };
            var conditionValidator = new ConditionValidator();

            // Act & Assert
            Assert.False(conditionValidator.IsValid(condition1, viewModel));
        }
    }
}