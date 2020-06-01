using System.Threading;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "addressManual")]
        public class AddressManualSteps : UiTestBase
        {
            [Then(@"I click the manual link")]
            [When(@"I click the manual link")]
            public void IClickTheManualLink()
            {
                BrowserSession.ClickLink("manual");
            }

        [Then(@"I fill in address line one")]
        public void ThenIFillInAddressLineOne()
        {
            BrowserSession.FillIn("customersaddresswithtitle-AddressLine1").With("test");
        }

        [Then(@"I fill in town")]
        public void ThenIFillInTown()
        {
            BrowserSession.FillIn("customersaddresswithtitle-AddressTown").With("town");
        }

        [Then(@"I fill in invalid postcode")]
        public void ThenIFillInInvalidPostcode()
        {
            BrowserSession.FillIn("customersaddresswithtitle-ManualPostcode").With("ahskdoen");
        }

        [Then(@"I fill in postcode")]
        public void ThenIFillInPostcode()
        {
            BrowserSession.FillIn("customersaddresswithtitle-ManualPostcode").With("sk1 3xe");
        }

        [Then(@"I fill in page2")]
        [When(@"I fill in page2")]
        public void ThenIFillInPage2()
        {
            BrowserSession.FillIn("customersaddressnotitle-postcode").With("sk1 1aa");
        }

        [Then(@"I fill in page3")]
        [When(@"I fill in page3")]
        public void ThenIFillInPage3()
        {
            BrowserSession.FillIn("optionaladdress-postcode").With("sk1 1aa");
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
    }
   }
