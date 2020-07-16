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

        [Then("I should see the header")]
        public void ThenIShouldSeeTheHeaderSection()
        { 
            Assert.True(BrowserSession.FindCss(".smbc-header").Exists());
            Assert.True(BrowserSession.FindCss(".smbc-header__link--homepage").Exists());
        }

        [Then("I should see the form title in the header")]
        public void ThenIShouldSeeFormTitle()
        {
            Assert.True(BrowserSession.FindCss("#formTitle").Exists());
        }

        [Then(@"I should find an element with class ""(.*)""")]
        public void ThenIShouldSeeElementWithClass(string className)
        {
            Assert.True(BrowserSession.FindCss(className).Exists());
        }

        [Then(@"I should not find an element with class ""(.*)""")]
        public void ThenIShouldNotSeeElementWithClass(string className)
        {
            Assert.False(BrowserSession.FindCss(className).Exists());
        }



        [Then("I should see the footer")]
        public void ThenIShouldSeeTheFooterSection()
        {
            Assert.True(BrowserSession.FindCss(".smbc-footer").Exists());
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

        [Then(@"I wait one second")]
        public void ThenIWaitOneSecond()
        {
            Thread.Sleep(1000);
        }

        [Then(@"I should see that ""(.*)"" input has value ""(.*)""")]
        public void ThenIShouldSeeTheInput(string inputName, string value)
        {
            Assert.True(BrowserSession.FindField(inputName).HasValue(value));
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
        [Then(@"I enter ""(.*)"" in ""(.*)""")]
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

        [Then(@"I should see a validation error with an id ""(.*)"" with ""(.*)"" text")]
        public void ThenIShouldSeeAValidationError_WithText(string id, string text)
        {
            var webDriver = BrowserSession.Native as IWebDriver;
            var input = webDriver.FindElement(By.Id(id));
            Assert.True(input.Displayed);
            Assert.Contains(text, input.Text);
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
