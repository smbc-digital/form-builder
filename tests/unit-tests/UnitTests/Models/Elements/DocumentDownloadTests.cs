using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class DocumentDownloadTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        [Fact]
        public async Task RenderAsync_ShouldUseDefaultDocumentDownloadText()
        {
            //Arrange
            var callback = new DocumentDownload();
            _mockIViewRender
                .Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<DocumentDownload>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, DocumentDownload, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            var element = new ElementBuilder()
                .WithType(EElementType.DocumentDownload)
                .WithDocumentType(EDocumentType.Txt)
                .Build();

            var textBoxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(textBoxElement)
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal($"Download {EDocumentType.Txt} document", callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "DocumentDownload"), It.IsAny<DocumentDownload>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}
