using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PayService;
using form_builder.Services.SubmitService;
using form_builder.Workflows.PaymentWorkflow;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Workflows
{
    public class PaymentWorkflowTests
    {
        private readonly PaymentWorkflow _workflow;
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<ISubmitService> _submitService = new();
        private readonly Mock<IPayService> _payService = new();
        private readonly Mock<IMappingService> _mappingService = new();

        public PaymentWorkflowTests()
        {
            _workflow = new PaymentWorkflow(_payService.Object, _submitService.Object, _mappingService.Object, _sessionHelper.Object);
        }

        [Fact]
        public async Task Submit_ShouldThrowApplicationException_WhenNoSessionGuid()
        {
            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _workflow.Submit("form", "page"));

            // Assert
            Assert.Equal("A Session GUID was not provided.", result.Message);
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Never);
            _submitService.Verify(_ => _.PaymentSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _payService.Verify(_ => _.ProcessPayment(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Submit_ShouldCallMapping_Submit_And_PayService()
        {
            // Arrange
            _sessionHelper.Setup(_ => _.GetBrowserSessionId()).Returns("123454");

            // Act
            await _workflow.Submit("form", "page");

            // Assert
            _mappingService.Verify(_ => _.Map(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Once);
            _submitService.Verify(_ => _.PreProcessSubmission(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _submitService.Verify(_ => _.PaymentSubmission(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _payService.Verify(_ => _.ProcessPayment(It.IsAny<MappingEntity>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}