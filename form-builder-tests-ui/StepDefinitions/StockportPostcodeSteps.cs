using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "stockportpostcode")]
    public class StockportPostcode : UiTestBase
    {
        [Then(@"I fill in page1 with wrong postcode")]
        public void ThenIFillInPage1WithWrongPostcode()
        {
            BrowserSession.FillIn("postcode").With("lu1 1ln");
        }

        [Then(@"I fill in page1 with good postcode")]
        public void ThenIFillInPage1WithGoodPostcode()
        {
            BrowserSession.FillIn("postcode").With("sk7 4nu");
        }
    }
}
