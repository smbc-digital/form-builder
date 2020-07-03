using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Services.RetrieveExternalDataService;
using form_builder.Workflows;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class ActionsWorkflowTests
    {
        private readonly ActionsWorkflow _actionsWorkflow;
        private readonly Mock<IRetrieveExternalDataService> _mockRetrieveExternalDataService = new Mock<IRetrieveExternalDataService>();

        public ActionsWorkflowTests()
        {
            _actionsWorkflow = new ActionsWorkflow(_mockRetrieveExternalDataService.Object);
        }

        [Fact]
        public async Task Process_ShouldCallService_IfRetrieveExternalDataActionExists()
        {
            // Arrange
            var page = new Page
            {
                PageActions = new List<PageAction>
                {
                    new PageAction
                    {
                        Type = EPageActionType.RetrieveExternalData
                    }
                }
            };

            // Act
            await _actionsWorkflow.Process(page, "form");

            // Assert
            _mockRetrieveExternalDataService.Verify(_ => _.Process(page.PageActions, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldNotCallService_IfRetrieveExternalDataActionDoesNotExists()
        {
            // Arrange
            var page = new Page
            {
                PageActions = new List<PageAction>
                {
                    new PageAction
                    {
                        Type = EPageActionType.Unknown
                    }
                }
            };

            // Act
            await _actionsWorkflow.Process(page, "form");

            // Assert
            _mockRetrieveExternalDataService.Verify(_ => _.Process(page.PageActions, It.IsAny<string>()), Times.Never);
        }
    }
}
