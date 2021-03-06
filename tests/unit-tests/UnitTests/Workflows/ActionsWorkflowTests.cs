﻿using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Services.EmailService;
using form_builder.Services.RetrieveExternalDataService;
using form_builder.Services.ValidateService;
using form_builder.Workflows.ActionsWorkflow;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class ActionsWorkflowTests
    {
        private readonly ActionsWorkflow _actionsWorkflow;
        private readonly Mock<IRetrieveExternalDataService> _mockRetrieveExternalDataService = new Mock<IRetrieveExternalDataService>();
        private readonly Mock<IEmailService> _mockEmailService = new Mock<IEmailService>();
        private readonly Mock<ISchemaFactory> _mockSchemaFactory = new Mock<ISchemaFactory>();
        private readonly Mock<IValidateService> _mockValidateService = new Mock<IValidateService>();

        public ActionsWorkflowTests()
        {
            _actionsWorkflow = new ActionsWorkflow(_mockRetrieveExternalDataService.Object, _mockEmailService.Object, _mockSchemaFactory.Object, _mockValidateService.Object);
        }

        [Fact]
        public async Task Process_ShouldCallService_IfRetrieveExternalDataActionExists()
        {
            // Arrange
            var page = new Page
            {
                PageActions = new List<IAction>
                {
                    new RetrieveExternalData
                    {
                        Type = EActionType.RetrieveExternalData
                    }
                }
            };

            // Act
            await _actionsWorkflow.Process(page.PageActions, new FormSchema(), "form");

            // Assert
            _mockRetrieveExternalDataService.Verify(_ => _.Process(page.PageActions, It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Process_ShouldCallService_IfValidateActionExists()
        {
            // Arrange
            var page = new Page
            {
                PageActions = new List<IAction>
                {
                    new Validate
                    {
                        Type = EActionType.Validate
                    }
                }
            };

            // Act
            await _actionsWorkflow.Process(page.PageActions, new FormSchema(), "form");

            // Assert
            _mockValidateService.Verify(_ => _.Process(page.PageActions, It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Once);
        }
    }
}