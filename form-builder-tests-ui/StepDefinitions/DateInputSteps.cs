using System;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "dateinput")]
    class DateInputSteps : UiTestBase
    {
        [Then(@"I fill the day with ""(.*)"" value, month with ""(.*)"" value and year with ""(.*)"" value on ""(.*)""")]
        public void ThenIFillTheDateInputWithStrings(string day, string month, string year, string questionId)
        {
            //Act
            BrowserSession.FillIn(questionId + "-day").With(day);
            BrowserSession.FillIn(questionId + "-month").With(month);
            BrowserSession.FillIn(questionId + "-year").With(year);
        }

        [Then(@"I fill the date input with today's date in ""(.*)""")]
        public void ThenIFillTheDateInputWithTodaysDate(string questionId)
        {
            // Arrange
            DateTime currentDate = DateTime.Today;

            //Act
            BrowserSession.FillIn(questionId + "-day").With(currentDate.Day.ToString());
            BrowserSession.FillIn(questionId + "-month").With(currentDate.Month.ToString());
            BrowserSession.FillIn(questionId + "-year").With(currentDate.Year.ToString());

        }

        [Then(@"I should see todays date refilled in the date input in for ""(.*)"" blah")]
        public void ThenIShouldSeeTodaysDateRefilledInTheDateInput(string questionId)
        {
            // Arrange
            DateTime currentDate = DateTime.Today;

            var dayValue = BrowserSession.FindId(questionId + "-day").Value;
            var monthValue = BrowserSession.FindId(questionId + "-month").Value;
            var yearValue = BrowserSession.FindId(questionId + "-year").Value;

            Assert.Equal(currentDate.Day.ToString(), dayValue);
            Assert.Equal(currentDate.Month.ToString(), monthValue);
            Assert.Equal(currentDate.Year.ToString(), yearValue);
        }

        [Then(@"I should not see a validation message for ""(.*)"" input")]
        public void ThenIShouldNotSeeValidationMessageForInput(string inputName)
        {
            //Assert
            Assert.False(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I should see a validation message for ""(.*)"" input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I should see the values ""(.*)"", ""(.*)"" and ""(.*)"" in the date input for ""(.*)"" blah")]
        public void ThenIShouldSeeEnteredValues(string expectedDayValue, string expectedMonthValue, string expectedYearValue, string questionId)
        {
            var dayValue = BrowserSession.FindId(questionId + "-day").Value;
            var monthValue = BrowserSession.FindId(questionId + "-month").Value;
            var yearValue = BrowserSession.FindId(questionId + "-year").Value;

            Assert.Equal(expectedDayValue, dayValue);
            Assert.Equal(expectedMonthValue, monthValue);
            Assert.Equal(expectedYearValue, yearValue);
        }

    }
}