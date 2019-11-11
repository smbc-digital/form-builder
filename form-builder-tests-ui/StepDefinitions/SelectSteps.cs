using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "select")]
    class SelectSteps : UiTestBase
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

        [Then(@"I click the ""(.*)"" radiobutton")]
        [When(@"I click the ""(.*)"" radiobutton")]
        public void ThenIClickTheRadioButton(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act
            webDriver.FindElement(By.Id(inputId)).Click();
        }

        [Then(@"The ""(.*)"" radiobutton should be checked")]
        public void TheradioButtonShouldBeChecked(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act

            //Assert
            Assert.True(webDriver.FindElement(By.Id(inputId)).Selected);
        }

        [Then(@"The ""(.*)"" radiobutton should be unchecked")]
        public void TheradioButtonShouldNotBeChecked(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act

            //Assert
            Assert.False(webDriver.FindElement(By.Id(inputId)).Selected);
        }

        [Then (@"I should see ""(.*)"" is selected in dropdown")]
        public void TheSelectedOptionShouldBeSelected(string value)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act

            //Assert
            //Assert.True(webDriver.FindElement(By.Id("tuesday")).Selected);
            Assert.Equal(value, BrowserSession.FindId("select").SelectedOption);

        }


    }
}