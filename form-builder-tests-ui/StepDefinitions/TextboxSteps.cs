using TechTalk.SpecFlow;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "textbox")]
    public class Textbox : UiTestBase
    {
        [Then(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("firstName").With("test");
            BrowserSession.FillIn("lastName").With("test");          
        }

        [Then(@"I press the next step button")]
        public void ThenIPressTheNextStepButton()
        {
            BrowserSession.ClickButton("nextStep");
        }
    }
}
