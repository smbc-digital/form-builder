using System.Threading;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "button")]
    public class ButtonSteps : UiTestBase
    {
        [Then(@"I should see an element with ID of ""(.*)""")]
        public void ThenIShouldSeeAElement(string elementType)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.True(webDriver.FindElement(By.Id($"{elementType}")).Displayed);
        }

        [Then(@"I wait one second")]
        public void ThenIWaitOneSecond()
        {
            Thread.Sleep(1000);
        }
    }
}
