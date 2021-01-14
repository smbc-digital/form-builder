using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
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
    public class LinkTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        public LinkTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderForLinkElement()
        {
            //Arrange
            Element element = new ElementBuilder()
                .WithType(EElementType.Link)
                .WithPropertyText("test")
                .WithUrl("test")
                .WithClassName("govuk-button")
                .WithOpenInTab(true)
                .Build();

            Page page = new PageBuilder()
                .WithElement(element)
                .Build();

            Dictionary<string, dynamic> viewModel = new Dictionary<string, dynamic>();

            FormSchema schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            FormAnswers formAnswers = new FormAnswers();
            //Act
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync<Element>(It.Is<string>(x => x.Equals("Link")), It.IsAny<Link>(), It.IsAny<Dictionary<string, object>?>()), Times.Once);
        }
    }
}
