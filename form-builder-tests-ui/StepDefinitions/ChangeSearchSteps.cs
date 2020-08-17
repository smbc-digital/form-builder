using TechTalk.SpecFlow;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding]
    class ChangeSearchSteps : UiTestBase
    {
        [When(@"I click the ""(.*)"" link")]
        [Then(@"I click the ""(.*)"" link")]
        public void WhenIClickTheLink(string name)
        {
            BrowserSession.ClickLink(name); 
        }
    }
}
