using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Models;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Services.PageService;
using form_builder.Services.PageService.Entities;
using form_builder.Workflows;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class SuccessWorkflowTests
    {
        private readonly SuccessWorkflow _workflow;
        private readonly Mock<IPageService> _mockPageService = new Mock<IPageService>();
        private readonly Mock<ISchemaFactory> _mockSchemaFactory = new Mock<ISchemaFactory>();
        private readonly Mock<IActionsWorkflow> _mockActionsWorkflow = new Mock<IActionsWorkflow>();

        public SuccessWorkflowTests()
        {
            _workflow = new SuccessWorkflow(_mockPageService.Object, _mockSchemaFactory.Object, _mockActionsWorkflow.Object);

            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithStartPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>())).ReturnsAsync(formSchema);
            _mockPageService.Setup(_ => _.FinalisePageJourney(It.IsAny<string>(), EBehaviourType.SubmitForm, It.IsAny<FormSchema>()))
                .ReturnsAsync(new SuccessPageEntity
                {
                    FormAnswers = new FormAnswers()
                });
        }

        [Fact]
        public async Task Process_ShouldCallSchemaFactory()
        {
            // Act
            await _workflow.Process(EBehaviourType.SubmitForm, "form");

            // Assert
            _mockSchemaFactory.Verify(_ => _.Build("form"), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldNotCallActionService()
        {
            // Act
            await _workflow.Process(EBehaviourType.SubmitForm, "form");

            // Assert
            _mockActionsWorkflow.Verify(_ => _.Process(It.IsAny<List<IAction>>(), It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Process_ShouldCallActionService()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithStartPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .WithFormActions(new UserEmail
                {
                    Properties = new BaseActionProperty(),
                    Type = EActionType.UserEmail
                })
                .Build();

            _mockSchemaFactory.Setup(_ => _.Build(It.IsAny<string>())).ReturnsAsync(formSchema);

            // Act
            await _workflow.Process(EBehaviourType.SubmitForm, "form");

            // Assert
            _mockActionsWorkflow.Verify(_ => _.Process(It.IsAny<List<IAction>>(), It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCallPageService()
        {
            // Act
            await _workflow.Process(EBehaviourType.SubmitForm, "form");

            // Assert
            _mockPageService.Verify(_ => _.FinalisePageJourney("form", EBehaviourType.SubmitForm, It.IsAny<FormSchema>()), Times.Once);
        }
    }
}