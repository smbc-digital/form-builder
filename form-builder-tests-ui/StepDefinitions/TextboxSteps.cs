using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "textbox")]
    public class Textbox : UiTestBase
    {
        [Then(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("firstName").With("test123456789");
            BrowserSession.FillIn("lastName").With("test");          
        }

        [Then(@"I should see a validation message for ""(.*)"" input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindId("firstName-error").Exists());
            Assert.True(BrowserSession.FindId("lastName-error").Exists());
        }
    }
}
