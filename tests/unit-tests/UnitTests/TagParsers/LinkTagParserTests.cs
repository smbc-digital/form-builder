using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParser;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class LinkTagParserTests
    {
        private readonly IEnumerable<IFormatter> _formatters;
        private readonly Mock<IFormatter> _mockFormatter = new Mock<IFormatter>();
        private LinkTagParser _tagParser;

        public LinkTagParserTests()
        {
            _tagParser = new LinkTagParser(_formatters);
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
        public void FormatContent_ShouldReturnValidFormatedText()
        {
            var url = "www.stockport.gov.uk";
            var linkText = "link text";
            var expectValue = string.Format(_tagParser._htmlContent, url, linkText);
            Assert.Equal(expectValue, _tagParser.FormatContent(new string[2]{url,linkText}));
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
        public void Parse_ShouldReturn_UpdatedText_WithReplacedValue()
        {
            var expectedString = $"this link {_tagParser.FormatContent(new string [2]{"www.stockport.gov", "text"})} should be replaced";

             var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this link {{LINK:www.stockport.gov:text}} should be replaced")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var result = _tagParser.Parse(page, new FormAnswers());
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
        }

    }
}