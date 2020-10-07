using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParser;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class FormAnswersAdditionalDataTagParserTests
    {
        private FormAnswersAdditionalDataTagParser _tagParser = new FormAnswersAdditionalDataTagParser();

        [Theory]
        [InlineData("{{ANSWERDATA:firstname}}")]
        [InlineData("{{ANSWERDATA:ref}}")]
        public void Regex_ShouldReturnTrue_Result(string value)
        {
            Assert.True(_tagParser.Regex.Match(value).Success);
        }

        [Theory]
        [InlineData("{{ANSWERDATAA:firstname}}")]
        [InlineData("{{NSWERDATA:ref}}")]
        [InlineData("{ANSWERDATA:firstname}")]
        [InlineData("{{ANSWERDATA:firstname}")]
        [InlineData("{{TAG:firstname}")]
        [InlineData("{{DIFFERENTTAG:firstname}}")]
        public void Regex_ShouldReturnFalse_Result(string value)
        {
            Assert.False(_tagParser.Regex.Match(value).Success);
        }

        [Fact]
        public void Parse_ShouldReturnInitalValue_WhenNoValuesAre_To_BeReplaced()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this has no values to be replaced")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var formAnswers = new FormAnswers();

            var result = _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public void Parse_ShouldReturnInitalValue_When_NoTag_MatchesRegex()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{TAG:firstname}} should be replaced with name question")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();
            
            var formAnswers = new FormAnswers();

            var result = _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public void Parse_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
        {
            var expectedString = "this value testfirstname should be replaced with name question";

             var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{ANSWERDATA:firstname}} should be replaced with name question")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                AdditionalFormAnswersData = new Dictionary<string, object> 
                {
                    { "firstname", "testfirstname"}
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public void Parse_ShouldReturnUpdatedValue_WhenReplacingMultipleValues()
         {
            var expectedString = "this value testfirstname should be replaced with firstname and this testlastname with lastname";

             var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{ANSWERDATA:firstname}} should be replaced with firstname and this {{ANSWERDATA:lastname}} with lastname")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                AdditionalFormAnswersData = new Dictionary<string, object> 
                {
                    { "firstname", "testfirstname"},
                    { "lastname", "testlastname"}
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
        }
    }
}