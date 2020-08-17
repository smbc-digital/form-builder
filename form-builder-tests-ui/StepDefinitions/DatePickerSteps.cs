using TechTalk.SpecFlow;
using Xunit;
using System.Threading;
using OpenQA.Selenium;


namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "datepicker")]
    class DatePickerSteps : UiTestBase
    {

        [Then(@"I should not see a validation message for ""(.*)"" date picker")]
        public void ThenIShouldNotSeeValidationMessageForInput(string inputName)
        {
            //Assert           
            Assert.False(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I should see a validation message for ""(.*)"" date picker")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I select ""(.*)"" on ""(.*)"" date picker")]
        [When(@"I select ""(.*)"" on ""(.*)"" date picker")]
        public void ThenISelectDateFromDatePicker(string date,string inputName)
        {
            BrowserSession.FindId(inputName).SendKeys(date);
        }

        [Then(@"I wait five seconds")]
        public void ThenIWaitFiveSeconds()
        {
            Thread.Sleep(5000);
        }
    }
}
