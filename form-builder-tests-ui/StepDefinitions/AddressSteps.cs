using TechTalk.SpecFlow;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "address")]
    public class Address : UiTestBase
    {
        [Then(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("customers-address-postcode").With("sk1 1aa");
        }
    }
}
