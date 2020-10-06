using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.ContentFactory;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.ContentFactory
{
    public class PageFactoryTests
    {
        private readonly PageFactory _factory;
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();

        public PageFactoryTests()
        {
            _factory = new PageFactory(_mockPageHelper.Object);
        }

        [Fact]
        public async Task Build_ShouldCallPageService_AndReturnsStringValue()
        {
            // Arrange
            var html = "testHtml";
            _mockPageHelper.Setup(_ => _.GenerateHtml(
                It.IsAny<Page>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<FormSchema>(),
                It.IsAny<string>(),
                It.IsAny<FormAnswers>(),
                It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel { RawHTML = html });

            // Act
            var result = await _factory.Build(new Page(), new Dictionary<string, dynamic>(), new FormSchema(), string.Empty, new FormAnswers());

            // Assert
            Assert.Equal(html, result.RawHTML);
            _mockPageHelper.Verify(_ => _.GenerateHtml(
                It.IsAny<Page>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<FormSchema>(),
                It.IsAny<string>(),
                It.IsAny<FormAnswers>(),
                It.IsAny<List<object>>()), Times.Once);
        }

        [Fact]
        public async Task Build_ShouldReturn_Correct_DataValues()
        {
            // Arrange
            var html = "testHtml";
            var pageUrl = "page-one";
            var startPageUrl = "start-page-url";

            _mockPageHelper.Setup(_ => _.GenerateHtml(
                It.IsAny<Page>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<FormSchema>(),
                It.IsAny<string>(),
                It.IsAny<FormAnswers>(),
                It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel { RawHTML = html });

            var formSchema = new FormSchemaBuilder()
                .WithBaseUrl("base")
                .WithName("form name")
                .WithFeedback("BETA", "feedbackurl")
                .WithStartPageUrl(startPageUrl)
                .Build();

            var formAnswers = new FormAnswers();

            var page = new PageBuilder()
                .WithPageSlug(pageUrl)
                .WithPageTitle("page title")
                .Build();

            // Act
            var result = await _factory.Build(page, new Dictionary<string, dynamic>(), formSchema, string.Empty, formAnswers);

            // Assert
            Assert.Equal(html, result.RawHTML);
            Assert.Equal(pageUrl, result.Path);
            Assert.Equal("form name", result.FormName);
            Assert.Equal("page title", result.PageTitle);
            Assert.Equal("feedbackurl", result.FeedbackForm);
            Assert.Equal("BETA", result.FeedbackPhase);
            Assert.Equal(startPageUrl, result.StartPageUrl);
        }
    }
}