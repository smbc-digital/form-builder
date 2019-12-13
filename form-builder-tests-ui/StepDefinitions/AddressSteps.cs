using TechTalk.SpecFlow;
using System.Threading;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "address")]
    public class Address : UiTestBase
    {
        [Then(@"I fill in page1")]
        [When(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("customers-address-postcode").With("sk1 1aa");
        }

        [Then(@"I fill in page2")]
        [When(@"I fill in page2")]
        public void ThenIFillInPage2()
        {
            BrowserSession.FillIn("optional-address-postcode").With("sk1 1aa");
        }

        [Then(@"I wait one second")]
        public void ThenIWaitOneSecond()
        {
            Thread.Sleep(1000);
        }
    }
}
