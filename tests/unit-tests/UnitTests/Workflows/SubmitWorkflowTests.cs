using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.SubmitService;
using form_builder.Workflows.SubmitWorkflow;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class SubmitWorkflowTests
    {
        private readonly SubmitWorkflow _workflow;
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<ISubmitService> _submitService = new();
        private readonly Mock<IMappingService> _mappingService = new();

        public SubmitWorkflowTests()
        {
            _sessionHelper
                .Setup(_ => _.GetBrowserSessionId())
                .Returns("123454");

            _mappingService
                .Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .ReturnsAsync(new MappingEntity { BaseForm = new FormSchema() });

            _workflow = new SubmitWorkflow(_submitService.Object, _mappingService.Object, _sessionHelper.Object);
        }

        [Fact]
        public async Task Submit_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Arrange
            _sessionHelper
                .Setup(_ => _.GetBrowserSessionId())
                .Returns("");

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _workflow.Submit("form"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Never);
            _submitService.Verify(_ => _.ProcessSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldCallMapping_And_SubmitService()
        {
            // Act
            await _workflow.Submit("form");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Once);
            _submitService.Verify(_ => _.ProcessSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SubmitWithoutSubmission_ShouldCallSessionHelper()
        {
           // Act
            await _workflow.SubmitWithoutSubmission("form");

            // Assert
            _sessionHelper.Verify(_ => _.GetBrowserSessionId(), Times.Once);
        }

        [Fact]
        public async Task SubmitWithoutSubmission_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Arrange
            _sessionHelper
                .Setup(_ => _.GetBrowserSessionId())
                .Returns("");

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _workflow.SubmitWithoutSubmission("form"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Never);
            _submitService.Verify(_ => _.ProcessSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SubmitWithoutSubmission_ShouldCallSubmitService_PreProcessSubmission()
        {
            // Act
            await _workflow.SubmitWithoutSubmission("form");

            // Assert
            _submitService.Verify(_ => _.PreProcessSubmission(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SubmitWithoutSubmission_ShouldCallMappingService()
        {
            // Act
            await _workflow.SubmitWithoutSubmission("form");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Once);
        }

        [Fact]
        public async Task SubmitWithoutSubmission_ShouldCallSubmitService_ProcessWithoutSubmission()
        {
            // Act
            await _workflow.SubmitWithoutSubmission("form");

            // Assert
            _submitService.Verify(_ => _.ProcessWithoutSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}