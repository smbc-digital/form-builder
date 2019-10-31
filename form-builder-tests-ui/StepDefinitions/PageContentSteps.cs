﻿using OpenQA.Selenium;
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
            Assert.True(webDriver.FindElement(By.XPath("//p/strong[text() = 'This is a paragraph']")).Displayed);
        }

        [Then(@"I should see a list within a p tag")]
        public void ThenIShouldSeeAList()
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.True(webDriver.FindElement(By.XPath("//p//child:ul//child:li[text() = 'list item 1']")).Displayed);
        }
    }
}
