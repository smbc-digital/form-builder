using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Services.EmailSubmitService;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Workflows.EmailWorkflow;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class EmailWorkflowTests
    {

        private readonly Mock<IMappingService> _mappingService = new();
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<IEmailSubmitService> _emailSubmitService = new();
        private readonly EmailWorkflow _emailWorkflow;
        public EmailWorkflowTests()
        {
            _emailWorkflow = new EmailWorkflow
            (
                _emailSubmitService.Object,
                _mappingService.Object,
                _sessionHelper.Object
            );
        }


        [Fact]
        public async Task Submit_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _emailWorkflow.Submit("form"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldCallMapping_And_SubmitService()
        {
            // Arrange
            _sessionHelper
                .Setup(_ => _.GetBrowserSessionId())
                .Returns("123454");
            _mappingService
                .Setup(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MappingEntity { BaseForm = new FormSchema() });
            _emailSubmitService
                .Setup(_ => _.EmailSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("12345678");

            // Act
            var result = await _emailWorkflow.Submit("form");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _emailSubmitService.Verify(_ => _.EmailSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal("12345678", _emailWorkflow.Submit("form").Result);
        }
    }
}
