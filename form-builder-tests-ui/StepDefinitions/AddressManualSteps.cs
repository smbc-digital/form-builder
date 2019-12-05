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
            BrowserSession.FillIn("customers-address-AddressManualAddressLine1").With("test");
        }

        [Then(@"I fill in town")]
        public void ThenIFillInTown()
        {
            BrowserSession.FillIn("customers-address-AddressManualAddressTown").With("town");
        }

        [Then(@"I fill in invalid postcode")]
        public void ThenIFillInInvalidPostcode()
        {
            BrowserSession.FillIn("customers-address-AddressManualAddressPostcode").With("ahskdoen");
        }

        [Then(@"I fill in postcode")]
        public void ThenIFillInPostcode()
        {
            BrowserSession.FillIn("customers-address-AddressManualAddressPostcode").With("sk1 3xe");
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
