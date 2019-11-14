using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "dateinput")]
    class DateInputSteps : UiTestBase
    {
        [Then(@"I fill the date input with strings")]
        public void ThenIFillTheDateInputWithStrings()
        {
            //Act
            BrowserSession.FillIn("passportIssued-day").With("aa");
            BrowserSession.FillIn("passportIssued-month").With("bb");
            BrowserSession.FillIn("passportIssued-year").With("cccc");
        }

        [Then(@"I should not see a validation message for ""(.*)"" input")]
        public void ThenIShouldNotSeeValidationMessageForInput(string inputName)
        {
            //Assert
            Assert.False(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I click the ""(.*)"" checkbox")]
        [When(@"I click the ""(.*)"" checkbox")]
        public void ThenIClickTheRadioButton(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act
            webDriver.FindElement(By.Id(inputId)).Click();
        }
    }
}