using System;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "yourdetails")]
    public class YourDetails : UiTestBase
    {
        [Then(@"I fill in your details")]
        public void ThenIFillInYourDetails()
        {
            BrowserSession.FillIn("firstName").With("test");
            BrowserSession.FillIn("lastName").With("test");
            BrowserSession.FillIn("middleName").With("test");
            BrowserSession.FillIn("address").With("test");           
        }

        [Then(@"I press the next step button")]
        public void ThenIPressTheNextStepButton()
        {
            BrowserSession.ClickButton("nextStep");
        }

        [Then(@"I fill in issue details")]
        public void ThenIFillinIssueDetails()
        {
            BrowserSession.FillIn("issueDetails").With("test");
        }

        [Then(@"I click the more details radio button")]
        public void ThenIClickTheMoreDetailsRadioButton()
        {
            BrowserSession.ClickButton("moreDetails-0");
        }
    }
}
