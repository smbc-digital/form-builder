using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "checkboxlist")]
    class CheckBoxListSteps : UiTestBase
    {
        [Then(@"I should see a validation message for ""(.*)"" input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            //Assert
            Assert.True(BrowserSession.FindId(inputName).Exists());
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

        [Then(@"The ""(.*)"" checkbox should be checked")]
        public void TheradioButtonShouldBeChecked(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act

            //Assert
            Assert.True(webDriver.FindElement(By.Id(inputId)).Selected);
        }

        [Then(@"The ""(.*)"" checkbox should be unchecked")]
        public void TheradioButtonShouldNotBeChecked(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act

            //Assert
            Assert.False(webDriver.FindElement(By.Id(inputId)).Selected);
        }

        [Then(@"The previous link is clicked")]
        public void ThePreviousLinkIsClicked()
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act

            //Assert
            webDriver.FindElement(By.ClassName("govuk-back-link")).Click();
        }
    }
}