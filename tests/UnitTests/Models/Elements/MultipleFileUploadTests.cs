using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class MultipleFileUploadests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        [Fact]
        public async Task RenderAsync_ShouldCall_ElementHelper_ToGetCurrentValue_AndRenderView()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("MultipleFileUpload")), It.IsAny<Map>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.CurrentValue<object>(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldSet_CurrentValue_WhenPageHelper_DoesNotReturnNull()
        {
            var currentAnswer = new List<FileUploadModel>{ new FileUploadModel() };
            
            var callBackValue = new MultipleFileUpload();
            _mockElementHelper.Setup(_ => _.CurrentValue<JArray>(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()))
                .Returns(JArray.FromObject(currentAnswer));

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<MultipleFileUpload>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, MultipleFileUpload, Dictionary<string, object>>((x, y, z) => callBackValue = y);

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("MultipleFileUpload")), It.IsAny<MultipleFileUpload>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.CurrentValue<object>(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()), Times.Once);
            Assert.NotEmpty(callBackValue.CurrentFilesUploaded);
            Assert.Single(callBackValue.CurrentFilesUploaded);
        }
    }
}
