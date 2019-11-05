using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "textarea")]
    public class Textarea : UiTestBase
    {
        [Then(@"I fill in page1")]
        public void ThenIFillInPage1()
        {
            BrowserSession.FillIn("issueOne").With("test data test data");
        }

        [Then(@"I fill in page2")]
        public void ThenIFillInPage2()
        {
            BrowserSession.FillIn("issueTwo").With("test data test data. test data test data. test data test data. test data test data. test data test data. test data test data. test data test data. test data test data.");
        }

        [Then(@"I press the next step button")]
        public void ThenIPressTheNextStepButton()
        {
            BrowserSession.ClickButton("nextStep");
        }

        [Then(@"I press the submit button")]
        public void ThenIPressTheSubmitButton()
        {
            BrowserSession.ClickButton("submit");
        }

        [Then(@"I should see a validation message for ""(.*)"" input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindId("issueTwo-error").Exists());
        }
    }
}
