using System;
using System.Linq;
using System.Threading;
using Coypu;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "street")]
    public class Street : UiTestBase
    {
        [Then(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("customers-street-street").With("Green");
        }

        [Then(@"I should see a ""(.*)"" element with ""(.*)"" text")]
        public void ThenIShouldSeeAElement(string elementType, string elementText)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.True(webDriver.FindElement(By.XPath($"//{elementType}[text() = '{elementText}']")).Displayed);
        }
    }
}
