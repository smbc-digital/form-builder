using TechTalk.SpecFlow;

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
        }
   }
