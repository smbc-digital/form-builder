using form_builder.Builders;
using form_builder.Constants;
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
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

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
            var section = new Section
            {
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
                .Callback<string, SummarySectionsViewModel, Dictionary<string, object>>((x, y, z) => callback = y);

            _mockElementHelper.Setup(_ => _.GenerateQuestionAndAnswersList(It.IsAny<string>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new List<PageSummary> { new PageSummary { PageSummaryId = "page-one", PageSlug = "page-one", Answers = new Dictionary<string, string> { { "question", "answer" } } } });

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
        public async Task RenderAsync_ShouldCall_ViewRender_WithCorrect_ViewModel_ForSummaryWith_Sections_And_AddAnother()
        {
            //Arrange
            var callback = new SummarySectionsViewModel();
            var sectionOne = new Section
            {
                Title = "title",
                Pages = new List<string> {
                    "page-one"
                }
            };

            var sectionTwo = new Section
            {
                Title = "addAnother",
                Pages = new List<string> {
                    "add-another"
                }
            };

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("name")
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithQuestionId("addAnother")
                .WithNestedElement(textboxElement)
                .Build();

            var summaryElement = new ElementBuilder()
                .WithType(EElementType.Summary)
                .withSummarySection(sectionOne)
                .withSummarySection(sectionOne)
                .withSummarySection(sectionTwo)
                .Build();

            var addAnotherPage = new PageBuilder()
                .WithPageSlug("add-another")
                .WithElement(addAnotherElement)
                .Build();

            var summaryPage = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(summaryElement)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithPage(addAnotherPage)
                .WithPage(summaryPage)
                .Build();

            var formAnswers = new FormAnswers { FormData = new Dictionary<string, object> { { $"{AddAnotherConstants.IncrementKeyPrefix}-addAnother", 1 } } };

            _mockIViewRender.Setup(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, SummarySectionsViewModel, Dictionary<string, object>>((x, y, z) => callback = y);

            _mockElementHelper.Setup(_ => _.GenerateQuestionAndAnswersList(It.IsAny<string>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new List<PageSummary>
                {
                    new PageSummary { PageSummaryId = "add-another", PageSlug = "add-another", Answers = new Dictionary<string, string> {{"question", "answer"}} },
                    new PageSummary { PageSummaryId = "add-another-addAnother-1", PageSlug = "add-another", Answers = new Dictionary<string, string> { { "question", "answer" } } },
                    new PageSummary { PageSummaryId = "page-one", PageSlug = "page-one", Answers = new Dictionary<string, string> { { "question", "answer" } } }
                });

            _mockElementHelper.Setup(_ => _.GetAddAnotherNumberOfFieldsets(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(1);

            //Act
            await summaryElement.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                summaryPage,
                schema,
                _mockHostingEnv.Object,
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Equal(4, callback.Sections.Count);
            Assert.Single(callback.Sections.Where(_ => _.Pages.Any(_ => _.PageSummaryId.Equals("add-another-addAnother-1"))));
            Assert.Equal(2, callback.Sections.Count(_ => _.Pages.Any(_ => _.PageSlug.Equals("add-another"))));
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
                .Callback<string, SummarySectionsViewModel, Dictionary<string, object>>((x, y, z) => callback = y);

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

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithCorrect_ViewModel_ForSummaryWithout_Sections_And_AddAnother()
        {
            //Arrange
            var callback = new SummarySectionsViewModel();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("name")
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithQuestionId("addAnother")
                .WithNestedElement(textboxElement)
                .Build();

            var addAnotherBehaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("summary")
                .Build();

            var summaryElement = new ElementBuilder()
                .WithType(EElementType.Summary)
                .Build();

            var summaryBehaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("success")
                .Build();

            var addAnotherPage = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(addAnotherElement)
                .WithBehaviour(addAnotherBehaviour)
                .Build();

            var summaryPage = new PageBuilder()
                .WithPageSlug("summary")
                .WithElement(summaryElement)
                .WithBehaviour(summaryBehaviour)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithPage(addAnotherPage)
                .WithPage(summaryPage)
                .Build();

            var formAnswers = new FormAnswers
            {
                FormData = new Dictionary<string, object> { { $"{AddAnotherConstants.IncrementKeyPrefix}addAnother", 1 } },
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>(),
                        PageSlug = "page-one"
                    }
                }
            };

            _mockIViewRender.Setup(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, SummarySectionsViewModel, Dictionary<string, object>>((x, y, z) => callback = y);

            _mockElementHelper.Setup(_ => _.GenerateQuestionAndAnswersList(It.IsAny<string>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new List<PageSummary>
                {
                    new PageSummary { PageSummaryId = "page-one", PageSlug = "page-one", Answers = new Dictionary<string, string> {{ "name", "answer"}} },
                    new PageSummary { PageSummaryId = "page-one-addAnother-1", PageSlug = "page-one", Answers = new Dictionary<string, string> { { "question", "answer" } } },
                    new PageSummary { PageSummaryId = "add-another", PageSlug = "add-another", Answers = new Dictionary<string, string> { { "question", "answer" } } }
                });

            _mockElementHelper.Setup(_ => _.GetAddAnotherNumberOfFieldsets(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(1);

            //Act
            await summaryElement.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                summaryPage,
                schema,
                _mockHostingEnv.Object,
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Equal(3, callback.Sections.Count);
            Assert.Single(callback.Sections.Where(_ => _.Pages.Any(_ => _.PageSummaryId.Equals("page-one-addAnother-1"))));
            Assert.Equal(2, callback.Sections.Count(_ => _.Pages.Any(_ => _.PageSlug.Equals("page-one"))));
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithCorrect_ViewModel_ForSummaryWithout_Sections_And_AddAnotherSkipped()
        {
            //Arrange
            var callback = new SummarySectionsViewModel();

            var textboxElementOne = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("question")
                .Build();

            var pageOneBehaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("summary")
                .Build();

            var pageOne = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(textboxElementOne)
                .WithBehaviour(pageOneBehaviour)
                .Build();

            var textboxElementTwo = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("name")
                .Build();

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithQuestionId("addAnother")
                .WithNestedElement(textboxElementTwo)
                .Build();

            var summaryElement = new ElementBuilder()
                .WithType(EElementType.Summary)
                .Build();

            var addAnotherPage = new PageBuilder()
                .WithPageSlug("add-another")
                .WithElement(addAnotherElement)
                .Build();

            var summaryBehaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("success")
                .Build();

            var summaryPage = new PageBuilder()
                .WithPageSlug("summary")
                .WithElement(summaryElement)
                .WithBehaviour(summaryBehaviour)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithPage(pageOne)
                .WithPage(addAnotherPage)
                .WithPage(summaryPage)
                .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>(),
                        PageSlug = "page-one"
                    }
                }
            };

            _mockIViewRender.Setup(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, SummarySectionsViewModel, Dictionary<string, object>>((x, y, z) => callback = y);

            _mockElementHelper.Setup(_ => _.GenerateQuestionAndAnswersList(It.IsAny<string>(), It.IsAny<FormSchema>()))
                .ReturnsAsync(new List<PageSummary>
                {
                    new PageSummary { PageSummaryId = "page-one", PageSlug = "page-one", Answers = new Dictionary<string, string> { { "question", "answer" } } }
                });

            _mockElementHelper.Setup(_ => _.GetAddAnotherNumberOfFieldsets(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(0);

            //Act
            await summaryElement.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                summaryPage,
                schema,
                _mockHostingEnv.Object,
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Summary")), It.IsAny<SummarySectionsViewModel>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Single(callback.Sections);
            Assert.DoesNotContain(callback.Sections, section => section.Pages.Any(page => page.PageSummaryId.Contains("add-another")));
        }
    }
}