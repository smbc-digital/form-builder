using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "breadcrumbs")]
    public class Breadcrumbs : UiTestBase
    {
        [Then(@"I fill in page0")]
        public void ThenIFillInPage0()
        {
            BrowserSession.FillIn("firstQuestion").With("test");
        }

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
