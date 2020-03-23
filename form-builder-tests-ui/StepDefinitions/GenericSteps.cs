using System;
using System.Linq;
using System.Threading;
using Coypu;
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

        [Then(@"I sleep ""(.*)""")]
        public void GivenISleep(int sleepTime)
        {
            Thread.Sleep(sleepTime);
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
            Assert.True(BrowserSession.FindAllCss("a[href*='https://www.stockport.gov.uk']").Any());
        }

        [Then("I should see the breadcrumbs")]
        public void ThenIShouldSeeBreadcrumbs()
        {
            Assert.True(BrowserSession.FindCss(".breadcrumb-container").Exists());
        }

        [Then("I should see the form title in the header")]
        public void ThenIShouldSeeFormTitle()
        {
            Assert.True(BrowserSession.FindCss("#formTitle").Exists());
        }

        [Then(@"I should find an element with class ""(.*)""")]
        public void ThenIShouldSeeBreadcrumbs(string className)
        {
            Assert.True(BrowserSession.FindCss(className).Exists());
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
        [Then(@"I click the ""(.*)"" button")]
        public void WhenIClickTheButton(string name)
        {
            BrowserSession.ClickButton(name, new Options { WaitBeforeClick = TimeSpan.FromSeconds(0.5) });
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

        [Then(@"I should not see a validation message for ""(.*)"" input")]
        public void ThenIShouldNotSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindCss(string.Format(".form-field-validation-error[data-valmsg-for='{0}']", inputName)).Exists());
        }

        [Then(@"I should not see any ""(.*)"" html element")]
        public void ThenIShouldNotSeeAnyHtmlElementWithClassName(string className)
        {
            Assert.False(BrowserSession.FindCss(className).Exists());
        }

        [Then(@"I should see a ""(.*)"" html element")]
        public void ThenIShouldSeeAHtmlElementWithClassName(string className)
        {
            Assert.True(BrowserSession.FindCss(className).Exists());
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

        [When(@"I select ""(.*)"" in ""(.*)"" dropdown")]
        [Then(@"I select ""(.*)"" in ""(.*)"" dropdown")]
        public void ThenISelectDropdown(string value, string fieldName)
        {
            BrowserSession.FindId(fieldName).SelectOption(value);
            
        }

        [Then(@"I should see a ""(.*)"" element with ""(.*)"" text")]
        public void ThenIShouldSeeAElement(string elementType, string elementText)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.True(webDriver.FindElement(By.XPath($"//{elementType}[text() = '{elementText}']")).Displayed);
        }

        [Then(@"I should not see a ""(.*)"" element with ""(.*)"" text")]
        public void ThenIShouldNotSeeAElement(string elementType, string elementText)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            Assert.False(webDriver.FindElement(By.XPath($"//{elementType}[text() = '{elementText}']")).Displayed);
        }

        [Then(@"I should see ""(.*)"" is selected in ""(.*)"" dropdown with the value ""(.*)""")]
        public void TheSelectedOptionShouldBeSelected(string text, string selectId, string value)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Assert
            Assert.Equal(text, BrowserSession.FindId(selectId).SelectedOption);
            Assert.Equal(value, BrowserSession.FindId(selectId).Value);
        }
    }
}
