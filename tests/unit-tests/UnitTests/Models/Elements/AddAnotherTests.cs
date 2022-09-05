using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class AddAnotherTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        [Fact]
        public async Task RenderAsync_ShouldReturnEmptyString()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal(string.Empty, result);
        }
    }
}
