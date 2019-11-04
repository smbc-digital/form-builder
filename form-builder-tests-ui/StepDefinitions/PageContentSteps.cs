using OpenQA.Selenium;
using System;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "pagecontent")]
    public class PageContent : UiTestBase
    {
        [Then(@"I should see a strong element within a p tag")]
        public void ThenIShouldSeeAStrongElement()
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var strongText = webDriver.FindElements(By.TagName("p"))[0].FindElement(By.TagName("strong"));

            Assert.Equal("This is strong text", strongText.Text);
        }

        [Then(@"I should see an image element within a p tag")]
        public void ThenIShouldSeeAnImageElement()
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var imageDisplayed = webDriver.FindElements(By.TagName("p"))[1].FindElement(By.TagName("img")).Displayed;

            Assert.True(imageDisplayed);
        }

        [Then(@"I should see a link element within a p tag")]
        public void ThenIShouldSeeALinkElement()
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var linkHref = webDriver.FindElements(By.TagName("p"))[3].FindElement(By.TagName("a")).GetAttribute("href");

            Assert.Equal("https://www.stockport.gov.uk/", linkHref);
        }
    }
}
