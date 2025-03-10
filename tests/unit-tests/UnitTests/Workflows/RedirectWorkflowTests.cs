using form_builder.Helpers.Session;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.SubmitService;
using form_builder.Workflows.RedirectWorkflow;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class RedirectWorkflowTests
    {
        private readonly RedirectWorkflow _workflow;
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<ISubmitService> _submitService = new();
        private readonly Mock<IMappingService> _mappingService = new();
        private readonly Mock<IDistributedCacheWrapper> _distributedCache = new();

        private readonly Mock<ILogger<RedirectWorkflow>> _logger = new();

        public RedirectWorkflowTests()
        {
            _workflow = new RedirectWorkflow(_submitService.Object, _mappingService.Object, _sessionHelper.Object, _distributedCache.Object, _logger.Object);
        }

        [Fact]
        public async Task Submit_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _workflow.Submit("form", "page"));

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Never);
            _submitService.Verify(_ => _.RedirectSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldCallMappingService_WhenSessionGuidIsSupplied()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("123454");

            // Act
            await _workflow.Submit("form", "page");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Once);
        }

        [Fact]
        public async Task Submit_ShouldCallSubmitService_WhenSessionGuidIsSupplied()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("123454");

            // Act
            await _workflow.Submit("form", "page");

            // Assert
            _submitService.Verify(_ => _.RedirectSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Submit_ShouldDeleteCacheEntry()
        {
            // Arrange
            var guid = "1234";
            _sessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns(guid);

            // Act
            await _workflow.Submit("form", "page");

            // Assert
            _submitService.Verify(_ => _.RedirectSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _distributedCache.Verify(_ => _.Remove(It.IsAny<string>()), Times.Once);
        }
    }
}
