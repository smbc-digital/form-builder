using System;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "yourdetails")]
    public class YourDetails : UiTestBase
    {
        [Then(@"I should see a Your Details Content")]
        public void ThenIShouldSeeALinkToAnArticle()
        {
            Assert.True(BrowserSession.HasContent("Your Details"));
        }

    }
}
