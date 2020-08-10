using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "LabelHideShow")]
    public class LabelHideShowSteps : UiTestBase
    {
        [Then(@"I should see a label element within a h1 tag with text ""(.*)""")]
        public void ThenIShouldSeeTheLabelElementWithinH1WithText(string expectedText)
        {
            var webDriver = BrowserSession.Native as IWebDriver;

            var strongText = webDriver.FindElement(By.TagName("h1")).FindElement(By.TagName("label"));

            Assert.Equal(expectedText, strongText.Text);
        }
    }
}
