using OpenQA.Selenium;
using System;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "pagecontent")]
    public class PageContent : UiTestBase
    {
        [Then(@"I should see an unordered list with list items")]
        public void ThenIShouldSeeAnUnorderedList()
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var unorderedList = webDriver.FindElement(By.TagName("ul")).FindElements(By.TagName("li"));

            Assert.Equal(3, unorderedList.Count);
            Assert.Equal("List Item 1", unorderedList[0].Text);
        }

        [Then(@"I should see an ordered list with list items")]
        public void ThenIShouldSeeAnOrderedList()
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var unorderedList = webDriver.FindElement(By.TagName("ol")).FindElements(By.TagName("li"));

            Assert.Equal(3, unorderedList.Count);
            Assert.Equal("Ordered List Item 1", unorderedList[0].Text);
        }

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
