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
            BrowserSession.FillIn("LastName").With("test");
            BrowserSession.FillIn("middleName").With("test");
            BrowserSession.FillIn("address").With("test");           
        }

    }
}
