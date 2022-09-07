using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers
{
    public class CaseReferenceTagParserTests
    {
        private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
        private readonly CaseReferenceTagParser _tagParser;

        public CaseReferenceTagParserTests()
        {
            _tagParser = new CaseReferenceTagParser(_mockFormatters.Object);
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
                .WithLeadingParagraph("this has no values to be replaced")
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(page.LeadingParagraph, result.LeadingParagraph);
        }

        [Fact]
        public async Task Parse_ShouldReturnInitialValue_When_NoTag_MatchesRegex()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("this value {{TAG}} should be replaced with Case Reference")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithLeadingParagraph("this value {{TAG}} should be replaced with Case Reference")
                .Build();

            var formAnswers = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(element.Properties.Text, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(page.LeadingParagraph, result.LeadingParagraph);
        }

        [Fact]
        public async Task Parse_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
        {
            var expectedString = "this value 123456 should be replaced with Case Reference";

            var element = new ElementBuilder()
               .WithType(EElementType.P)
               .WithPropertyText("this value {{CASEREFERENCE}} should be replaced with Case Reference")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithLeadingParagraph("this value {{CASEREFERENCE}} should be replaced with Case Reference")
                .Build();

            var formAnswers = new FormAnswers
            {
                CaseReference = "123456"
            };

            var result = await _tagParser.Parse(page, formAnswers);

            Assert.Equal(expectedString, result.Elements.FirstOrDefault().Properties.Text);
            Assert.Equal(expectedString, result.LeadingParagraph);
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
            var text = "this value {{TAG}} should be replaced with Case Reference";
            var formAnswers = new FormAnswers();

            var result = _tagParser.ParseString(text, formAnswers);

            Assert.Equal(text, result);
        }

        [Fact]
        public void ParseString_ShouldReturnUpdatedValue_WhenReplacingSingleValue()
        {
            var expectedString = "this value 123456 should be replaced with Case Reference";

            var text = "this value {{CASEREFERENCE}} should be replaced with Case Reference";

            var formAnswers = new FormAnswers
            {
                CaseReference = "123456"
            };

            var result = _tagParser.ParseString(text, formAnswers);

            Assert.Equal(expectedString, result);
        }
    }
}
