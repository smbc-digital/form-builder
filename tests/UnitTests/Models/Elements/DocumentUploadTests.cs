using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class DocuemntUploadTest
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        [Fact]
        public async Task RenderAsync_ShouldCallGenerateDocumentUploadUrl_Base64StringCaseRef()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DocumentUpload)
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
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("DocumentUpload")), It.IsAny<DocumentUpload>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.GenerateDocumentUploadUrl(It.IsAny<Element>(), It.IsAny<FormSchema>(), It.IsAny<FormAnswers>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldSet_DocumentUploadUrl()
        {
            var url = "test";

            var callBackValue = new DocumentUpload();
            _mockElementHelper.Setup(_ => _.GenerateDocumentUploadUrl(It.IsAny<Element>(), It.IsAny<FormSchema>(), It.IsAny<FormAnswers>()))
                .Returns(url);

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<DocumentUpload>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, DocumentUpload, Dictionary<string, object>>((x, y, z) => callBackValue = y);

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DocumentUpload)
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
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("DocumentUpload")), It.IsAny<DocumentUpload>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.GenerateDocumentUploadUrl(It.IsAny<Element>(), It.IsAny<FormSchema>(), It.IsAny<FormAnswers>()), Times.Once);
            Assert.NotEmpty(callBackValue.Properties.DocumentUploadUrl);
            Assert.Equal(url, callBackValue.Properties.DocumentUploadUrl);
        }
    }
}
