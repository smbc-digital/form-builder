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
        private readonly Mock<IPayService> _paymentService = new();

        public PaymentAmountTagParserTests()
        {
            _paymentService.Setup(_ => _.GetFormPaymentInformation(It.IsAny<string>(), It.IsAny<Page>()))
                           .ReturnsAsync(new PaymentInformation() { Settings = new Settings { Amount = "£1"} });

            _tagParser = new PaymentAmountTagParser(_mockFormatters.Object, _paymentService.Object);
        }

        [Fact]
        public void Parse_ShouldReturnPaymentAmount()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("{{PaymentAmount}}")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var formAnswer = new FormAnswers();

            var result = _tagParser.Parse(page, formAnswer);

            Assert.True(result.Elements.Select(_ => _.Properties.Text.Equals("£1")).FirstOrDefault());
        }
    }
}
