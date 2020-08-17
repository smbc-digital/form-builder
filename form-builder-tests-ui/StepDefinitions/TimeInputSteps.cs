using System;
using TechTalk.SpecFlow;
using Xunit;
using OpenQA.Selenium;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "timeinput")]
    class TimeInputSteps : UiTestBase
    {
        [Then(@"I fill the hours with ""(.*)"" value, minutes with ""(.*)"" value and ampm with ""(.*)"" value on ""(.*)""")]
        public void ThenIFillTheTimeInputWithStrings(string hours, string minutes, string amPm, string questionId)
        {
            //Act
            BrowserSession.FillIn(questionId + "-hours").With(hours);
            BrowserSession.FillIn(questionId + "-minutes").With(minutes);

            var webDriver = BrowserSession.Native as IWebDriver;

            //Act
            webDriver.FindElement(By.Id(questionId + "-am")).Click();
        }

      

        [Then(@"I should see time refilled in the time input with ""(.*)"" value, minutes with ""(.*)"" value in for ""(.*)"" blah")]
        public void ThenIShouldSeeTimeRefilledInTheTimeInput(string hours, string minutes, string questionId)
        {
            // Arrange
            var hoursValue = BrowserSession.FindId(questionId + "-hours").Value;
            var minutesValue = BrowserSession.FindId(questionId + "-minutes").Value;

            // Assert
            Assert.Equal(hours, hoursValue);
            Assert.Equal(minutes, minutesValue);
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

        [Then(@"I should see the values ""(.*)"", ""(.*)"" and ""(.*)"" in the time input for ""(.*)"" blah")]
        public void ThenIShouldSeeEnteredValues(string expectedHoursValue, string expectedMinutesValue, string expectedAmPmValue, string questionId)
        {
            var hoursValue = BrowserSession.FindId(questionId + "-hours").Value;
            var monthValue = BrowserSession.FindId(questionId + "-month").Value;
            var ampmValue = BrowserSession.FindId(questionId + $"-{expectedAmPmValue}").Selected;

            Assert.Equal(expectedHoursValue, hoursValue);
            Assert.Equal(expectedMinutesValue, monthValue);
            Assert.True(ampmValue);
        }

    }
}