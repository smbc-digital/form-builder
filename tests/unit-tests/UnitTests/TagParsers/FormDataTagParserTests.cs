using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers
{
    public class FormDataTagParserTests
    {
        private readonly IEnumerable<IFormatter> _formatters;
        private readonly Mock<IFormatter> _mockFormatter = new Mock<IFormatter>();
        private FormDataTagParser _tagParser;

        public FormDataTagParserTests()
        {
            _mockFormatter.Setup(_ => _.FormatterName).Returns("testformatter");
            _mockFormatter.Setup(_ => _.Parse(It.IsAny<string>())).Returns("FAKE-FORMATTED-VALUE");
            _formatters = new List<IFormatter>
            {
                _mockFormatter.Object
            };

            _tagParser = new FormDataTagParser(_formatters);
        }

        [Theory]
        [InlineData("{{FORMDATA:firstname}}")]
        [InlineData("{{FORMDATA:ref}}")]
        public void Regex_ShouldReturnTrue_Result(string value)
        {
            Assert.True(_tagParser.Regex.Match(value).Success);
        }

        [Theory]
        [InlineData("{{FORMDATAA:firstname}}")]
        [InlineData("{{ORMDATA:ref}}")]
        [InlineData("{FORMDATA:firstname}")]
        [InlineData("{{FORMDATA:firstname}")]
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
               .WithPropertyText("this value {{FORMDATA:firstname}} should be replaced with name question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                AdditionalFormData = new Dictionary<string, object>
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
               .WithPropertyText("this value {{FORMDATA:firstname}} should be replaced with firstname and this {{FORMDATA:lastname}} with lastname")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                AdditionalFormData = new Dictionary<string, object>
                {
                    { "firstname", "testfirstname"},
                    { "lastname", "testlastname"}
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
        }


        [Fact]
        public void Parse_ShouldCallFormatter_WhenProvided()
        {
            var expectedString = "this value should be formatted: FAKE-FORMATTED-VALUE";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value should be formatted: {{FORMDATA:firstname:testformatter}}")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswers = new FormAnswers
            {
                AdditionalFormData = new Dictionary<string, object>
                {
                    { "firstname", "testfirstname"}
                }
            };

            var result = _tagParser.Parse(page, formAnswers);
            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            _mockFormatter.Verify(_ => _.Parse(It.IsAny<string>()), Times.Once);
        }

    }
}