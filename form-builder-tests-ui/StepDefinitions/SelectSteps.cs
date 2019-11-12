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

        [Then (@"I should see ""(.*)"" is selected in dropdown with the value ""(.*)""")]
        public void TheSelectedOptionShouldBeSelected(string text, string value)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act

            //Assert
            Assert.Equal(text, BrowserSession.FindId("select").SelectedOption);
            Assert.Equal(value, BrowserSession.FindId("select").Value);
        }


    }
}