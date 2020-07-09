using form_builder.Conditions;
using form_builder.Enum;
using form_builder.Models;
using System;
using System.Collections.Generic;
using Xunit;


namespace form_builder_tests.UnitTests.Conditions
{
    public class ConditionTestsWithComparisonValue
    {
        [Fact]
        public void Equal_To_Should_Return_True_False()
        {
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear");
            viewModel.Add("test2", "apple");

            var condition1 = new Condition { ComparisonValue = "pear", QuestionId = "test", ConditionType = ECondition.EqualTo };
            var condition2 = new Condition { ComparisonValue = "plum", QuestionId = "test2", ConditionType = ECondition.EqualTo };

            var conditionValidator = new ConditionValidator();
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
            Assert.False(conditionValidator.IsValid(condition2, viewModel));
        }


        [Fact]
        public void CheckBox_Contains_Should_Return_True_False()
        {
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "pear,guava");
            viewModel.Add("test2", "apple,mango");

            var condition1 = new Condition { ComparisonValue = "pear", QuestionId = "test", ConditionType = ECondition.CheckboxContains };
            var condition2 = new Condition { ComparisonValue = "plum", QuestionId = "test2", ConditionType = ECondition.CheckboxContains };

            var conditionValidator = new ConditionValidator();
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
            Assert.False(conditionValidator.IsValid(condition2, viewModel));
        }

        [Fact]
        public void Is_Null_Or_Empy_Should_Return_True_False()
        {
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test", "");
            viewModel.Add("test2", "apple,mango");

            var condition1 = new Condition { ComparisonValue = "true", QuestionId = "test", ConditionType = ECondition.IsNullOrEmpty };
            var condition2 = new Condition { ComparisonValue = "false", QuestionId = "test2", ConditionType = ECondition.IsNullOrEmpty };

            var conditionValidator = new ConditionValidator();
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
            Assert.True(conditionValidator.IsValid(condition2, viewModel));
        }

        [Fact]
        public void Is_Before_Should_Return_True()
        {
            var futureDate = DateTime.Today.AddDays(4);
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", futureDate.Day.ToString());
            viewModel.Add("test-month", futureDate.Month.ToString());
            viewModel.Add("test-year", futureDate.Year.ToString());

            var condition1 = new Condition { ComparisonValue = "10", Unit = EDateUnit.Day, ComparisonDate="Today", QuestionId = "test", ConditionType = ECondition.IsBefore };

            var conditionValidator = new ConditionValidator();
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void Is_Before_Should_Return_False()
        {
            var futureDate = DateTime.Today.AddDays(20);
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", futureDate.Day.ToString());
            viewModel.Add("test-month", futureDate.Month.ToString());
            viewModel.Add("test-year", futureDate.Year.ToString());

            var condition1 = new Condition { ComparisonValue = "10" , Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsBefore };

            var conditionValidator = new ConditionValidator();
            Assert.False(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void Is_After_Should_Return_False()
        {
            var futureDate = DateTime.Today.AddDays(2);
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", futureDate.Day.ToString());
            viewModel.Add("test-month", futureDate.Month.ToString());
            viewModel.Add("test-year", futureDate.Year.ToString());

            var condition1 = new Condition { ComparisonValue = "10", Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsAfter };

            var conditionValidator = new ConditionValidator();
            Assert.False(conditionValidator.IsValid(condition1, viewModel));
        }

        [Fact]
        public void Is_After_Should_Return_True()
        {
            var futureDate = DateTime.Today.AddDays(20);
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-day", futureDate.Day.ToString());
            viewModel.Add("test-month", futureDate.Month.ToString());
            viewModel.Add("test-year", futureDate.Year.ToString());

            var condition1 = new Condition { ComparisonValue = "10", Unit = EDateUnit.Day, ComparisonDate = "Today", QuestionId = "test", ConditionType = ECondition.IsAfter };

            var conditionValidator = new ConditionValidator();
            Assert.True(conditionValidator.IsValid(condition1, viewModel));
        }
    }
}
