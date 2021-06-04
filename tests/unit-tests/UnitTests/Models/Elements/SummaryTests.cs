using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class SummaryTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new ();
        private readonly Mock<IElementHelper> _mockElementHelper = new ();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new ();

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_ToRenderSummaryView()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithCorrect_ViewModel_ForSummaryWith_Sections()
        {
            //Arrange
            var callback = new SummarySectionsViewModel();
            var section = new Section {
                Title = "title",
                Pages = new List<string> {
                    "page-one"
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .withSummarySection(section)
                .withSummarySection(section)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithPage(page)
                .Build();

            var formAnswers = new FormAnswers();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, SummarySectionsViewModel, Dictionary<string, object>>((x,y,z) => callback = y);

            _mockElementHelper.Setup(_ => _.GenerateQuestionAndAnswersList(It.IsAny<string>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new List<PageSummary>{new PageSummary { PageSlug = "page-one" }});

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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Equal(2, callback.Sections.Count);
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithCorrect_ViewModel_ForSummaryWithout_Sections()
        {
            //Arrange
            var callback = new SummarySectionsViewModel();
            var element = new ElementBuilder()
                .WithType(EElementType.Summary)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithPage(page)
                .Build();

            var formAnswers = new FormAnswers();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, SummarySectionsViewModel, Dictionary<string, object>>((x,y,z) => callback = y);

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
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Single(callback.Sections);
        }
    }
}