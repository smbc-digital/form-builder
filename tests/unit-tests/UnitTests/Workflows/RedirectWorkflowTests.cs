﻿using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.SubmitService;
using form_builder.Workflows.RedirectWorkflow;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class RedirectWorkflowTests
    {
        private readonly RedirectWorkflow _workflow;
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<ISubmitService> _submitService = new();
        private readonly Mock<IMappingService> _mappingService = new();

        public RedirectWorkflowTests()
        {
            _workflow = new RedirectWorkflow(_submitService.Object, _mappingService.Object, _sessionHelper.Object);
        }

        [Fact]
        public async Task Submit_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _workflow.Submit("form", "page"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _submitService.Verify(_ => _.RedirectSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldCallMappingService_WhenSessionGuidIsSupplied()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");

            // Act
            await _workflow.Submit("form", "page");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Submit_ShouldCallSubmitService_WhenSessionGuidIsSupplied()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");

            // Act
            await _workflow.Submit("form", "page");

            // Assert
            _submitService.Verify(_ => _.RedirectSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}