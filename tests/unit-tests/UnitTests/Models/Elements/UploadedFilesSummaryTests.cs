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
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class UploadedFilesSummaryTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        [Fact]
        public async Task RenderAsync_ShouldCall_PageHelper_ToGetCurrentValue()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string> { "fileone", "file-two" })
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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("UploadedFilesSummary")), It.IsAny<UploadedFilesSummary>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            _mockElementHelper.Verify(_ => _.CurrentValue<object>(It.Is<string>(_ => _.Equals("fileone")), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()), Times.Once);
            _mockElementHelper.Verify(_ => _.CurrentValue<object>(It.Is<string>(_ => _.Equals("file-two")), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_Should_PopulateListItems_Values()
        {
            var currentAnswer = new List<FileUploadModel> { new FileUploadModel { TrustedOriginalFileName = "test.jpg" } };
            var callback = new UploadedFilesSummary();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<UploadedFilesSummary>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, UploadedFilesSummary, Dictionary<string, object>>((x, y, z) => callback = y);

            _mockElementHelper.Setup(_ => _.CurrentValue<JArray>(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(JArray.FromObject(currentAnswer));

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string> { "fileone", "file-two" })
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
            Assert.Equal(2, callback.Properties.ListItems.Count);
        }

        [Fact]
        public async Task RenderAsync_Should_PopulateListItems_WhenValueIsNotNull_OrEmpty()
        {
            var currentAnswer = new List<FileUploadModel> { new FileUploadModel { TrustedOriginalFileName = "test.jpg" } };
            var currentAnswerTwo = new List<FileUploadModel>();
            var callback = new UploadedFilesSummary();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<UploadedFilesSummary>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, UploadedFilesSummary, Dictionary<string, object>>((x, y, z) => callback = y);

            _mockElementHelper.Setup(_ => _.CurrentValue<JArray>(It.Is<string>(_ => _.Equals("fileone")), It.IsAny<Dictionary<string, object>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(JArray.FromObject(currentAnswer));

            _mockElementHelper.Setup(_ => _.CurrentValue<JArray>(It.Is<string>(_ => _.Equals("file-two")), It.IsAny<Dictionary<string, object>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns(JArray.FromObject(currentAnswerTwo));

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string> { "fileone", "file-two" })
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
            Assert.Single(callback.Properties.ListItems);
        }
    }
}