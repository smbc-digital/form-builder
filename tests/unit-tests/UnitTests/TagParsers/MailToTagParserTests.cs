using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers
{
    public class MailToTagParserTests
    {
        private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
        private readonly MailToTagParser _tagParser;

        public MailToTagParserTests()
        {
            _tagParser = new MailToTagParser(_mockFormatters.Object);
        }

        [Theory]
        [InlineData("{{MAILTO:firstname@asdfasdf.com}}")]
        [InlineData("{{MAILTO:ref@sadf.com}}")]
        public void Regex_ShouldReturnTrue_Result(string value)
        {
            Assert.True(_tagParser.Regex.Match(value).Success);
        }

        [Theory]
        [InlineData("{{Mail:firstname@asas.xom}}")]
        [InlineData("{{MAIL2:ref@sdsd.copm}}")]
        [InlineData("{MAILTO:@firstname.com}")]
        [InlineData("{{MAILTO:firstname}")]
        [InlineData("{{MAILTO4:firstname@asdfd.com}}")]
        [InlineData("{{DIFFERENTTAG:firstname}}")]
        public void Regex_ShouldReturnFalse_Result(string value)
        {
            Assert.False(_tagParser.Regex.Match(value).Success);
        }

        [Fact]
        public void FormatContent_ShouldReturnValidFormattedText()
        {
            var url = "john.smith@stockport.gov.uk";
            var expectValue = string.Format(_tagParser._htmlContent, url);
            Assert.Equal(expectValue, _tagParser.FormatContent(new string[1] { url }));
        }

        [Fact]
        public async Task Parse_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this has no values to be replaced")
                .Build();

            var textElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithHint("this has no values to be replaced")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(textElement)
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault(element => element.Type.Equals(EElementType.P)).Properties.Text);
            Assert.Equal(textElement.Properties.Hint, result.Elements.FirstOrDefault(element => element.Type.Equals(EElementType.Textbox)).Properties.Hint);
        }

        [Fact]
        public async Task Parse_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{TAG:firstname}} should be replaced with name question")
                .Build();

            var textElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithHint("this value {{TAG:firstname}} should be replaced with name question")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(textElement)
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault(element => element.Type.Equals(EElementType.P)).Properties.Text);
            Assert.Equal(textElement.Properties.Hint, result.Elements.FirstOrDefault(element => element.Type.Equals(EElementType.Textbox)).Properties.Hint);
        }

        [Fact]
        public async Task Parse_ShouldReturn_UpdatedText_WithReplacedValue()
        {
            var expectedString = $"this link {_tagParser.FormatContent(new string[1] { "john.smith@stockport.gov.uk" })} should be replaced";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this link {{MAILTO:john.smith@stockport.gov.uk}} should be replaced")
               .Build();

            var textElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithHint("this link {{MAILTO:john.smith@stockport.gov.uk}} should be replaced")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(textElement)
                .Build();

            var result = await _tagParser.Parse(page, new FormAnswers());
            Assert.Equal(expectedString, result.Elements.FirstOrDefault(element => element.Type.Equals(EElementType.P)).Properties.Text);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault(element => element.Type.Equals(EElementType.Textbox)).Properties.Hint);
        }

        [Fact]
        public void ParseString_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
        {
            var text = "this has no values to be replaced";
            var formAnswers = new FormAnswers();

            var result = _tagParser.ParseString(text, formAnswers);

            Assert.Equal(text, result);
        }

        [Fact]
        public void ParseString_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
        {
            var text = "this value {{TAG:firstname@yahoo.com}} should be replaced with name question";
            var formAnswers = new FormAnswers();

            var result = _tagParser.ParseString(text, formAnswers);

            Assert.Equal(text, result);
        }

        [Fact]
        public void ParseString_ShouldReturn_UpdatedText_WithReplacedValue()
        {
            var expectedString = $"this link {_tagParser.FormatContent(new string[1] { "john.smith@stockport.gov.uk" })} should be replaced";

            var text = "this link {{MAILTO:john.smith@stockport.gov.uk}} should be replaced";

            var result = _tagParser.ParseString(text, new FormAnswers());
            Assert.Equal(expectedString, result);
        }
    }
}