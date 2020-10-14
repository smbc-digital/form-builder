using System;
using System.Threading.Tasks;
using form_builder.Controllers.Document;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Workflows.DocumentWorkflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Controllers
{
    public class DocumentControllerTests
    {
        private readonly DocumentController _controller;
        private readonly Mock<ILogger<DocumentController>> _mockLogger = new Mock<ILogger<DocumentController>>();
        private readonly Mock<IDocumentWorkflow> _mockDocumentWorkflow = new Mock<IDocumentWorkflow>();

        public DocumentControllerTests()
        {
            _controller = new DocumentController(_mockDocumentWorkflow.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Summary_ShouldRedirect_ToError_WhenInvalidIdProvider()
        {
            var result = await _controller.Summary(EDocumentType.Txt, Guid.Empty);

            Assert.IsType<RedirectToActionResult>(result);
            _mockDocumentWorkflow.Verify(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Summary_ShouldRedirect_ToExpired_When_ServiceThrows_DocumentExpiredExceptionException()
        {
            _mockDocumentWorkflow.Setup(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<Guid>()))
                .ThrowsAsync(new DocumentExpiredException("an exception"));
            
            var result = await _controller.Summary(EDocumentType.Txt, Guid.NewGuid());

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Expired", redirectResult.ActionName);
            _mockDocumentWorkflow.Verify(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task Summary_ShouldReturnFile_OnSuccess()
        {
            _mockDocumentWorkflow.Setup(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<Guid>()))
                .ReturnsAsync(new byte[0]);
            
            var result = await _controller.Summary(EDocumentType.Txt, Guid.NewGuid());

            var fileContentResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("summary.txt", fileContentResult.FileDownloadName);
            Assert.Equal("text/plain", fileContentResult.ContentType);
            _mockDocumentWorkflow.Verify(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}
