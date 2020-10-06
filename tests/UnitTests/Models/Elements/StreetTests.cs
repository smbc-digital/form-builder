using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class StreetTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();
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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("StreetSelect")), It.IsAny<form_builder.Models.Elements.Street>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("StreetSearch")),It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}