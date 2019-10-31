using OpenQA.Selenium;
using System;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "pagecontent")]
    public class PageContent : UiTestBase
    {
        [Then(@"h1 should be displayed")]
        public void ThenIFillInYourDetails()
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.True(webDriver.FindElement(By.XPath("//h1[text() = 'Page content']")).Displayed);
        }

        [Then(@"I should see a ""(.*)"" element with ""(.*)"" text")]
        public void ThenIShouldSeeAElement(string elementType, string elementText)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.True(webDriver.FindElement(By.XPath($"//{elementType}[text() = '{elementText}']")).Displayed);
        }
    }
}
