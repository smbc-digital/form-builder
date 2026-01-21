using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers
{
    public class LinkTagParserTests
    {
        private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
        private readonly LinkTagParser _tagParser;

        public LinkTagParserTests()
        {
            _tagParser = new LinkTagParser(_mockFormatters.Object);
        }

        [Theory]
        [InlineData("{{LINK:firstname}}")]
        [InlineData("{{LINK:ref}}")]
        public void Regex_ShouldReturnTrue_Result(string value)
        {
            Assert.True(_tagParser.Regex.Match(value).Success);
        }

        [Theory]
        [InlineData("{{LIN:firstname}}")]
        [InlineData("{{INK:ref}}")]
        [InlineData("{LINK:firstname}")]
        [InlineData("{{LINK:firstname}")]
        [InlineData("{{TAG:firstname}")]
        [InlineData("{{DIFFERENTTAG:firstname}}")]
        public void Regex_ShouldReturnFalse_Result(string value)
        {
            Assert.False(_tagParser.Regex.Match(value).Success);
        }

        [Fact]
        public void FormatContent_ShouldReturnValidFormattedText()
        {
            var url = "www.stockport.gov.uk";
            var linkText = "link text";
            var expectValue = string.Format(_tagParser._htmlContent, url, linkText);
            Assert.Equal(expectValue, _tagParser.FormatContent(new string[2] { url, linkText }));
        }

        [Fact]
        public async Task Parse_ShouldReturnInitialValue_WhenNoValuesAre_To_BeReplaced()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this has no values to be replaced")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public async Task Parse_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{TAG:firstname}} should be replaced with name question")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public async Task Parse_ShouldReturn_UpdatedText_WithReplacedValue()
        {
            var expectedString = $"this link {_tagParser.FormatContent(new string[2] { "www.stockport.gov", "text" })} should be replaced";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this link {{LINK:www.stockport.gov:text}} should be replaced")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = await _tagParser.Parse(page, new FormAnswers());
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
        }

        [Fact]
        public async Task Parse_ShouldReturn_UpdatedList_WithReplacedValue()
        {
            // Arrange
            var expected = _tagParser.FormatContent(new[] { "www.stockport.gov", "text" });

            var element = new ElementBuilder()
                .WithType(EElementType.UL)
                .WithListItems(
                [
                    "first item",
                    "link item {{LINK:www.stockport.gov:text}}",
                    "last item"
                ])
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            // Act
            var result = await _tagParser.Parse(page, new FormAnswers());
            var updatedItems = result.Elements.First().Properties.ListItems;

            // Assert
            Assert.Equal("first item", updatedItems[0]);
            Assert.Equal($"link item {expected}", updatedItems[1]);
            Assert.Equal("last item", updatedItems[2]);
        }

        [Fact]
        public async Task Parse_ShouldReturn_InitialList_WhenNoTagsMatch()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithListItems(
                [
                    "first",
                    "second",
                    "third"
                ])
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = await _tagParser.Parse(page, new FormAnswers());

            Assert.Equal(element.Properties.ListItems, result.Elements.First().Properties.ListItems);
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
            var text = "this value {{TAG:firstname}} should be replaced with name question";
            var formAnswers = new FormAnswers();

            var result = _tagParser.ParseString(text, formAnswers);

            Assert.Equal(text, result);
        }

        [Fact]
        public void ParseString_ShouldReturn_UpdatedText_WithReplacedValue()
        {
            var expectedString = $"this link {_tagParser.FormatContent(new string[2] { "www.stockport.gov", "text" })} should be replaced";

            var text = "this link {{LINK:www.stockport.gov:text}} should be replaced";

            var result = _tagParser.ParseString(text, new FormAnswers());
            Assert.Equal(expectedString, result);
        }
    }
}