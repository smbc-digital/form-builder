using TechTalk.SpecFlow;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "street")]
    public class Street : UiTestBase
    {
        [Then(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("street-address-street").With("Green");
        }
    }
}
