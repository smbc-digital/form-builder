using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class AddressManualTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        public AddressManualTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ElementHelper_ToGetCurrentValue_ForAddressFields()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.AddressManual)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic> { { AddressManualConstants.POSTCODE, "sk11aa" } };

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();
            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "AddressManual"), It.IsAny<AddressManual>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.CurrentValue(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()), Times.Exactly(4));
        }

        [Fact]
        public async Task RenderAsync_Should_DisplayAddressIAG_WhenNoAddressFound_InSearchResults()
        {
            //Arrange
            var callbackElement = new AddressManual();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<AddressManual>(), It.IsAny<Dictionary<string, dynamic>>()))
                                .Callback<string, AddressManual, Dictionary<string, dynamic>>((a, b, c) => callbackElement = b);

            var element = new ElementBuilder()
                .WithType(EElementType.AddressManual)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();
            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
            new List<object>()
                );

            //Assert
            Assert.True(callbackElement.Properties.DisplayNoResultsIAG);
        }
    }
}