using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;


namespace form_builder_tests_ui.StepDefinitions
{
    [Binding]
    public class GenericSteps : UiTestBase
    {
        [Given(@"I navigate to ""(.*)""")]
        [When(@"I navigate to ""(.*)""")]
        public void GivenINavigateTo(string url)
        {
            BrowserSession.Visit(url);
            Thread.Sleep(1000);
        }

        [Given("I have signed in as UiTest")]
        public static void IHaveSignedIn()
        {
            BrowserSession.Visit("/this-is-a-invalid-url-to-allow-adding-a-cookie");
            var webDriver = BrowserSession.Native as IWebDriver;

            webDriver.Manage().Cookies.AddCookie(new Cookie("jwtCookie", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiVUkgVGVzdCIsImVtYWlsIjoic2NuLnVpdGVzdEBnbWFpbC5jb20ifQ.-LHnC83W_1jEC9PZALbQBYifzNLYwryUgi0GimD8SWY", "127.0.0.1", "/", DateTime.Now.AddDays(1)));
        }

        [Then("I should see the header")]
        public void ThenIShouldSeeTheHeaderSection()
        {
            ThenIShouldSeeTheLink("myaccount.stockport.gov.uk");
            //Assert.True(BrowserSession.FindAllCss("[class*='search-button']").Any());
            Assert.True(BrowserSession.FindAllCss("a[href*='https://www.stockport.gov.uk']").Any());
        }

        [Then("I should see the breadcrumbs")]
        public void ThenIShouldSeeBreadcrumbs()
        {
            Assert.True(BrowserSession.FindCss(".breadcrumb-container").Exists());
        }

        [Then("I should see the footer")]
        public void ThenIShouldSeeTheFooterSection()
        {
            Assert.True(BrowserSession.FindCss(".atoz").Exists());
            Assert.True(BrowserSession.FindCss(".l-container-footer").Exists());
            Assert.True(BrowserSession.FindCss(".cc_banner.cc_container.cc_container--open").Exists());
        }

        [Then("I should see the pagination section")]
        public void ThenIShouldSeeThePagination()
        {
            Assert.True(BrowserSession.FindAllCss(".pagination-section").Any());
        }

        [Then(@"I should see the ""(.*)"" link")]
        public void ThenIShouldSeeTheLink(string href)
        {
            Assert.True(BrowserSession.FindAllCss(string.Format("a[href*='{0}']", href)).Any());
        }

        [When(@"I click the ""(.*)"" button")]
        public void WhenIClickTheButton(string name)
        {
            BrowserSession.ClickButton(name);
        }

        [Then(@"I should see the ""(.*)"" button")]
        public void ThenIShouldSeeTheButton(string name)
        {
            Assert.True(BrowserSession.FindButton(name).Exists());
        }

        [When("I click the close alert button")]
        public void WhenIClickTheCloseAlertButton()
        {
            BrowserSession.FindAllCss(".alert-close a").FirstOrDefault().Click();
        }

        [Then(@"I should see the ""(.*)"" input")]
        public void ThenIShouldSeeTheInput(string inputName)
        {
            Assert.True(BrowserSession.FindField(inputName).Exists());
        }

        [Then(@"I should see the ""(.*)"" ""(.*)"" radio button")]
        public void ThenIShouldSeetheRadioButton(string fieldset, string value)
        {
            Assert.True(BrowserSession.FindFieldset(fieldset).FindField(value).Exists());
        }

        [Then(@"I should see the ""(.*)"" fieldset")]
        public void ThenIShouldSeeTheFieldset(string fieldset)
        {
            Assert.True(BrowserSession.FindFieldset(fieldset).Exists());
        }

        [Then(@"I should see a validation message for ""(.*)"" input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindCss(string.Format(".form-field-validation-error[data-valmsg-for='{0}']", inputName)).Exists());
        }

        [When(@"I enter ""(.*)"" in ""(.*)""")]
        public void WhenIEnter(string value, string fieldName)
        {
            BrowserSession.FillIn(fieldName).With(value);
        }

        [When(@"I select ""(.*)"" in ""(.*)""")]
        [Then(@"I select ""(.*)"" in ""(.*)""")]
        public void ThenISelect(string value, string fieldName)
        {
            BrowserSession.Select(value).From(fieldName);
        }
    }
}
