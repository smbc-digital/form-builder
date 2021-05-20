using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PayService;
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
        private readonly Mock<IPayService> _mockPaymentService = new();

        public PaymentAmountTagParserTests()
        {
            _mockPaymentService.Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>()))
                           .ReturnsAsync(new PaymentInformation() { Settings = new Settings { Amount = "10.00"} });

            _tagParser = new PaymentAmountTagParser(_mockFormatters.Object, _mockPaymentService.Object);
        }

        [Fact]
        public void Parse_ShouldReturnPaymentAmount()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("{{PAYMENTAMOUNT}}")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithLeadingParagraph("{{PAYMENTAMOUNT}}")
                .Build();

            var formAnswer = new FormAnswers();

            var result = _tagParser.Parse(page, formAnswer);

            Assert.Equal("10.00",result.Elements.First().Properties.Text);
            Assert.Equal("10.00", result.LeadingParagraph);
        }

        [Fact]
        public void Parse_ShouldCallPayService_IfAmountNotInFormAnswers()
        {
            var page = new PageBuilder()
                .WithLeadingParagraph("{{PAYMENTAMOUNT}}")
                .Build();

            var formAnswer = new FormAnswers();

            var result = _tagParser.Parse(page, formAnswer);

            _mockPaymentService.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Parse_ShouldNotCallPayService_IfAmountInFormAnswers()
        {
            var page = new PageBuilder()
                .WithLeadingParagraph("{{PAYMENTAMOUNT}}")
                .Build();

            var formAnswer = new FormAnswers
            {
                PaymentAmount = "15.00"
            };

            var result = _tagParser.Parse(page, formAnswer);

            _mockPaymentService.Verify(_ => _.GetFormPaymentInformation(It.IsAny<string>()), Times.Never);
        }
    }
}
