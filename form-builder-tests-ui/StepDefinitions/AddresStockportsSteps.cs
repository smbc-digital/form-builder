using OpenQA.Selenium;
using TechTalk.SpecFlow;
using System.Threading;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "addressstockport")]
    public class AddressStockportOnly : UiTestBase
    {
        [Then(@"I fill in page1")]
        [When(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("customersaddress-postcode").With("sk1 1aa");
        }

        [Then(@"I fill in page1 with invalid postcode")]
        [When(@"I fill in page1 with invalid postcode")]
        public void ThenIFillInPage1WithInvalidPostcode()
        {
            BrowserSession.FillIn("stockportaddressOne-postcode").With("B74 4OE");
        }

        [Then(@"I should see a validation message for ""(.*)"" input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            //Assert
            Assert.True(BrowserSession.FindId(inputName).Exists());
        }

        [Then(@"I fill in page2")]
        [When(@"I fill in page2")]
        public void ThenIFillInPage2()
        {
            BrowserSession.FillIn("optionaladdress-postcode").With("sk1 1aa");
        }

        [Then(@"I wait one second")]
        public void ThenIWaitOneSecond()
        {
            Thread.Sleep(1000);
        }

        [Then(@"I wait five seconds")]
        public void ThenIWaitFiveSeconds()
        {
            Thread.Sleep(6000);
        }
    }
}
