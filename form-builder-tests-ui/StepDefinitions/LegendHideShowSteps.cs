using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "LegendHideShow")]
    class LegendHideShowSteps : UiTestBase
    {
        [Then(@"I click the ""(.*)"" radiobutton")]
        [When(@"I click the ""(.*)"" radiobutton")]
        public void ThenIClickTheRadioButton(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act
            webDriver.FindElement(By.Id(inputId)).Click();
        }

        [Then(@"I should not see the h1 element with page title text")]
        public void ThenIShouldSeeAnImgElement()
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var theTitle = webDriver.FindElements(By.TagName("H1"))[0].Text;

            Assert.NotEqual("Page 2", theTitle);
        }

        [Then(@"I should see a legend element within a h1 tag with text ""(.*)""")]
        public void ThenIShouldSeeAStrongElement(string expectedText)
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var strongText = webDriver.FindElements(By.TagName("legend"))[0].FindElement(By.TagName("h1"));

            Assert.Equal(expectedText, strongText.Text);
        }
    }
}
