using System;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "dateinput")]
    class DateInputSteps : UiTestBase
    {
        [Then(@"I fill the day with ""(.*)"" value, month with ""(.*)"" value and year with ""(.*)"" value")]
        public void ThenIFillTheDateInputWithStrings(string day, string month, string year)
        {
            //Act
            BrowserSession.FillIn("passportIssued-day").With(day);
            BrowserSession.FillIn("passportIssued-month").With(month);
            BrowserSession.FillIn("passportIssued-year").With(year);
        }

        [Then(@"I fill the date input with today's date")]
        public void ThenIFillTheDateInputWithTodaysDate()
        {
            // Arrange
            DateTime currentDate = DateTime.Today;

            //Act
            BrowserSession.FillIn("passportIssued-day").With(currentDate.Day.ToString());
            BrowserSession.FillIn("passportIssued-month").With(currentDate.Month.ToString());
            BrowserSession.FillIn("passportIssued-year").With(currentDate.Year.ToString());

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

    }
}