using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "summary")]
    class SummarySteps : UiTestBase
    {
        [Then(@"I fill in page1")]
        [When(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("firstName").With("first");
            BrowserSession.FillIn("lastName").With("last");
            BrowserSession.FillIn("email").With("test@mail.com");
            BrowserSession.FillIn("phone").With("01614451234");
        }


        [Then(@"I fill in page3")]
        [When(@"I fill in page3")]
        public void ThenIFillInPage3()
        {
            BrowserSession.FillIn("dob-day").With("01");
            BrowserSession.FillIn("dob-month").With("02");
            BrowserSession.FillIn("dob-year").With("1990");
        }

        [Then(@"I click the ""(.*)"" radiobutton")]
        [When(@"I click the ""(.*)"" radiobutton")]
        public void ThenIClickTheRadioButton(string inputId)
        {
            //Arrange
            var webDriver = BrowserSession.Native as IWebDriver;

            //Act
            webDriver.FindElement(By.Id(inputId)).Click();
        }

        [When(@"I click the ""(.*)"" link")]
        [Then(@"I click the ""(.*)"" link")]
        public void WhenIClickTheLink(string name)
        {
            BrowserSession.ClickLink(name);
        }
    }
}
