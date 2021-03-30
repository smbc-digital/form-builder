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
using Newtonsoft.Json.Linq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class MultipleFileUploadTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new ();
        private readonly Mock<IElementHelper> _mockElementHelper = new ();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new ();

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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("MultipleFileUpload")), It.IsAny<MultipleFileUpload>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.CurrentValue<object>(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldSet_CurrentValue_WhenPageHelper_DoesNotReturnNull()
        {
            var currentAnswer = new List<FileUploadModel> { new FileUploadModel() };

            var callBackValue = new MultipleFileUpload();
            _mockElementHelper.Setup(_ => _.CurrentValue<JArray>(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("MultipleFileUpload")), It.IsAny<MultipleFileUpload>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.CurrentValue<object>(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()), Times.Once);
            Assert.NotEmpty(callBackValue.CurrentFilesUploaded);
            Assert.Single(callBackValue.CurrentFilesUploaded);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(10)]
        public void GenerateElementProperties_Should_Generate_ElementProperties(int value)
        {
            //Arrange
            var questionId = "filetest";
            var element = new ElementBuilder()
                .WithType(EElementType.MultipleFileUpload)
                .WithQuestionId(questionId)
                .WithMaxFileSize(value)
                .Build();

            //Act
            var result = element.GenerateElementProperties();

            //Assert
            Assert.Equal(7, result.Count);
            Assert.True(result.ContainsKey("data-individual-file-size"));
            Assert.True(result.ContainsKey("data-module"));
            Assert.True(result.ContainsKey("multiple"));
            Assert.True(result.ContainsKey("multiple"));
            Assert.True(result.ContainsKey("type"));
            Assert.True(result.ContainsKey("accept"));
            Assert.True(result.ContainsKey("id"));
            Assert.True(result.ContainsKey("name"));
            Assert.True(result.ContainsValue($"{questionId}{FileUploadConstants.SUFFIX}"));
            Assert.True(result.ContainsValue("file"));
            Assert.True(result.ContainsValue(true));
            Assert.True(result.ContainsValue("smbc-multiple-file-upload"));
            Assert.True(result.ContainsValue(value * SystemConstants.OneMBInBinaryBytes));
        }
    }
}
