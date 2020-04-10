using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "HideTitle")]
    class HideTitleSteps : UiTestBase
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

            var h1Count = webDriver.FindElements(By.TagName("h1")).Count;
            
            Assert.Equal(0,h1Count);
        }
    }
}
