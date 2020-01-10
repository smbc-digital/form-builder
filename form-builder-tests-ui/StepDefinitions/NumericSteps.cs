using TechTalk.SpecFlow;
using Xunit;

namespace form_builder_tests_ui.StepDefinitions
{
    [Binding, Scope(Tag = "numeric")]
    public sealed class NumericSteps :UiTestBase
    {
       

        [Then(@"I fill in in page 1 with incorrect numeric values")]
        [When(@"I fill in in page 1 with incorrect numeric values")]
        public void ThenIFillInPageIncorectNumericValues()
        {
            BrowserSession.FillIn("positiveInteger").With("-123");
            BrowserSession.FillIn("negativeInteger").With("1234");
            BrowserSession.FillIn("rangedInteger").With("12345");
            BrowserSession.FillIn("rangedIntegerOptional").With("12345");
        }


        [Then(@"I fill in in page 1 with non numeric values")]
        [When(@"I fill in in page 1 with non numeric values")]
        public void henIFillInPageNonNumericValues()
        {
            BrowserSession.FillIn("positveInteger").With("adfds");
            BrowserSession.FillIn("negativeInteger").With("asd4");
            BrowserSession.FillIn("rangedInteger").With("adsdfdsf");
            BrowserSession.FillIn("rangedIntegerOptional").With("dafdfdsf");
        }

        [Then(@"I should see a validation message for ""(.*)"" numeric input")]
        public void ThenIShouldSeeValidationMessageForInput(string inputName)
        {
            Assert.True(BrowserSession.FindId(inputName+$"-error").Exists());           
        }

    }
}
