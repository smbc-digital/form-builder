using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.TagParser;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class TagParserTests
    {
        private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new Mock<IEnumerable<IFormatter>>();
        private Regex _regex => new Regex("(?<={{)TEST:.*?(?=}})");
        private TagParser _tagParser;

        public TagParserTests(){
            _tagParser = new TagParser(_mockFormatters.Object);
        }

        [Fact]
        public void Parse_ShouldThrowException_WhenQuestionValue_IsNotWithinAnswers()
        {
            var result = Assert.Throws<ApplicationException>(() => _tagParser.Parse("{{TEST:question}}", new Dictionary<string, object>(), _regex));
            Assert.Equal("FormAnswerTagParser::Parse, replacement value for quetionId question is not stored within answers, Match value: TEST:question", result.Message);
        }

        [Fact]
        public void Parse_ShouldThrowException_WhenQuestionValue_IsNullOrEmpty()
        {
            var answers = new Dictionary<string, object>() 
            {
                { "question", string.Empty}
            };

            var result = Assert.Throws<ApplicationException>(() => _tagParser.Parse("{{TEST:question}}", answers, _regex));
            Assert.Equal("FormAnswerTagParser::Parse, replacement value for quetionId question is null or empty, Match value: TEST:question", result.Message);
        }
    }
}