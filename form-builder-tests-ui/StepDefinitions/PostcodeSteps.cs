using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "postcode")]
    public class Postcode : UiTestBase
    {
        [Then(@"I fill in page1 with bad postcode")]
        public void ThenIFillInPage1WithBadPostcode()
        {
            BrowserSession.FillIn("postcode").With("notapostcode");
        }

        [Then(@"I fill in page1 with good postcode")]
        public void ThenIFillInPage1WithGoodPostcode()
        {
            BrowserSession.FillIn("postcode").With("lu1 1ln");
        }
    }
}
