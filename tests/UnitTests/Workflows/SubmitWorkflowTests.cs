using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.SubmtiService;
using form_builder.Workflows;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class SubmitWorkflowTests
    {
        private readonly SubmitWorkflow _workflow;
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<ISubmitService> _submitService = new Mock<ISubmitService>();
        private readonly Mock<IMappingService> _mappingService = new Mock<IMappingService>();

        public SubmitWorkflowTests()
        {
            _workflow = new SubmitWorkflow(_submitService.Object, _mappingService.Object, _sessionHelper.Object);
        }

        [Fact]
        public async Task Submit_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _workflow.Submit("form"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _submitService.Verify(_ => _.ProcessSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldCallMapping_And_SubmitService()
        {
            _sessionHelper.Setup(_ => _.GetSessionGuid()).Returns("123454");

            // Act
            var result = await _workflow.Submit("form");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _submitService.Verify(_ => _.ProcessSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
