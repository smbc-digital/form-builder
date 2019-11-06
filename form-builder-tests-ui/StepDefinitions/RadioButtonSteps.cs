using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "radiobutton")]
    class RadioButtonSteps : UiTestBase
    {
        [Then(@"I press the submit button")]
        public void ThenIPressTheSubmitButton()
        {
            BrowserSession.ClickButton("submit");
        }


        [Then(@"I should see a validation message for ""(.*)"" input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I should not see a validation message for ""(.*)"" input")]
        public void ThenIShouldNotSeeValidationMessageForInput(string inputName)
        {
            Assert.False(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I click the ""(.*)"" radiobutton")]
        [When(@"I click the ""(.*)"" radiobutton")]
        public void ThenIClickTheRadioButton(string inputId)
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            webDriver.FindElement(By.Id(inputId)).Click();
        }

        [Then(@"The ""(.*)"" radiobutton should be checked")]
        public void TheradioButtonShouldBeChecked(string inputId)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.True(webDriver.FindElement(By.Id(inputId)).Selected);
        }

        [Then(@"The ""(.*)"" radiobutton should be unchecked")]
        public void TheradioButtonShouldNotBeChecked(string inputId)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.False(webDriver.FindElement(By.Id(inputId)).Selected);
        }
    }
}
