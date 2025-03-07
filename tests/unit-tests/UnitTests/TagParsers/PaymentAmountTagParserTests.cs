using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Models;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers
{
    public class PaymentAmountTagParserTests
    {
        private readonly Mock<IEnumerable<IFormatter>> _mockFormatters = new();
        private readonly PaymentAmountTagParser _tagParser;
        private readonly Mock<IPaymentHelper> _mockPaymentHelper = new();

        public PaymentAmountTagParserTests()
        {
            _mockPaymentHelper
                .Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new PaymentInformation { Settings = new Settings { Amount = "10.00" } });

            _tagParser = new PaymentAmountTagParser(_mockFormatters.Object, _mockPaymentHelper.Object);
        }

        [Fact]
        public async Task Parse_ShouldReturnPaymentAmount()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("{{PAYMENTAMOUNT}}")
                .Build();

            var condition = new ConditionBuilder()
                .WithQuestionId("{{PAYMENTAMOUNT}}")
                .WithComparisonValue("10.00")
                .WithConditionType(ECondition.PaymentAmountEqualTo)
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithCondition(condition)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithLeadingParagraph("{{PAYMENTAMOUNT}}")
                .WithBehaviour(behaviour)
                .Build();

            var formAnswer = new FormAnswers();

            var result = await _tagParser.Parse(page, formAnswer);

            Assert.Equal("10.00", result.Elements.First().Properties.Text);
            Assert.Equal("10.00", result.LeadingParagraph);
            Assert.Equal("10.00", result.Behaviours.FirstOrDefault().Conditions.FirstOrDefault().QuestionId);
        }

        [Fact]
        public void Parse_ShouldCallPaymentHelper_IfAmountNotInFormAnswers()
        {
            var page = new PageBuilder()
                .WithLeadingParagraph("{{PAYMENTAMOUNT}}")
                .Build();

            var formAnswer = new FormAnswers();

            _tagParser.Parse(page, formAnswer);

            _mockPaymentHelper.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()), Times.Once);
        }

        [Fact]
        public void Parse_ShouldNotCallPaymentHelper_IfAmountInFormAnswers()
        {
            var page = new PageBuilder()
                .WithLeadingParagraph("{{PAYMENTAMOUNT}}")
                .Build();

            var formAnswer = new FormAnswers
            {
                PaymentAmount = "15.00"
            };

            _tagParser.Parse(page, formAnswer);

            _mockPaymentHelper.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()), Times.Never);
        }

        [Fact]
        public void ParseString_ShouldReturnPaymentAmount()
        {
            var text = "{{PAYMENTAMOUNT}}";
            var formAnswer = new FormAnswers();

            var result = _tagParser.ParseString(text, formAnswer);

            Assert.Equal("10.00", result);
        }

        [Fact]
        public void ParseString_ShouldCallPaymentHelper_IfAmountNotInFormAnswers()
        {
            var text = "{{PAYMENTAMOUNT}}";
            var formAnswer = new FormAnswers();

            _tagParser.ParseString(text, formAnswer);

            _mockPaymentHelper.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()), Times.Once);
        }

        [Fact]
        public void ParseString_ShouldNotCallPaymentHelper_IfAmountInFormAnswers()
        {
            var text = "{{PAYMENTAMOUNT}}";

            var formAnswer = new FormAnswers
            {
                PaymentAmount = "15.00"
            };

            _tagParser.ParseString(text, formAnswer);

            _mockPaymentHelper.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<FormSchema>()), Times.Never);
        }
    }
}
