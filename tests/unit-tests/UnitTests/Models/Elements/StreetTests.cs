using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class StreetTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        public StreetTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task Street_ShouldCallViewRenderWithCorrectPartial_WhenStreetSearch()
        {
            // Arrange
            var element = new Street { Properties = new BaseProperty { Text = "text", QuestionId = "street" } };

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    LookUpConstants.SubPathViewModelKey,
                    LookUpConstants.Automatic
                },
                {"street-streetaddress", string.Empty},
                {"street-street", "street"}
            };

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
                .Build();

            var formAnswers = new FormAnswers();
            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            // Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("StreetSelect")), It.IsAny<Street>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task Street_ShouldCallViewRenderWithCorrectPartial_WhenStreetSelect()
        {
            //Arrange
            var element = new Street { Properties = new BaseProperty { Text = "text", QuestionId = "street", StreetProvider = "test" } };
            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("StreetSearch")), It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_IsDefined()
        {
            var label = "Custom label";
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Street)
                .WithQuestionId("street-test")
                .WithSummaryLabel(label)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal(label, result);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_Is_Not_Defined()
        {
            var pageTitle = "Page Title";
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Street)
                .WithQuestionId("street-test")
                .Build();

            //Act
            var result = element.GetLabelText(pageTitle);

            //Assert
            Assert.Equal(pageTitle, result);
        }
    }
}