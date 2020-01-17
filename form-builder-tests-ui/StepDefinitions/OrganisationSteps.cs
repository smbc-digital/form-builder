using System.Threading;
using TechTalk.SpecFlow;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "organisation")]
    public class Organisation : UiTestBase
    {
        [Then(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("organisation-organisation-searchterm").With("test org");
        }

        [Then(@"I fill in page2")]
        public void ThenIFillInPage2()
        {
            BrowserSession.FillIn("optorganisation-organisation-searchterm").With("test org optional");
        }

        [Then(@"I wait one second")]
        public void ThenIWaitOneSecond()
        {
            Thread.Sleep(1000);
        }
    }
}
