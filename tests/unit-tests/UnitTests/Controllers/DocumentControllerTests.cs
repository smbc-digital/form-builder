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
        private readonly Mock<ILogger<DocumentController>> _mockLogger = new();
        private readonly Mock<IDocumentWorkflow> _mockDocumentWorkflow = new();

        public DocumentControllerTests()
        {
            _controller = new DocumentController(_mockDocumentWorkflow.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Summary_ShouldRedirect_ToError_WhenInvalidIdProvider()
        {
            var result = await _controller.Summary(EDocumentType.Txt, null);

            Assert.IsType<RedirectToActionResult>(result);
            _mockDocumentWorkflow.Verify(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Summary_ShouldRedirect_ToExpired_When_ServiceThrows_DocumentExpiredException()
        {
            _mockDocumentWorkflow.Setup(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<string>()))
                .ThrowsAsync(new DocumentExpiredException("an exception"));

            var result = await _controller.Summary(EDocumentType.Txt, "This is some text");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Expired", redirectResult.ActionName);
            _mockDocumentWorkflow.Verify(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Summary_ShouldReturnFile_OnSuccess_ForTxt()
        {
            _mockDocumentWorkflow.Setup(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());

            var result = await _controller.Summary(EDocumentType.Txt, "This is some text");

            var fileContentResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("summary.txt", fileContentResult.FileDownloadName);
            Assert.Equal("text/plain", fileContentResult.ContentType);
            _mockDocumentWorkflow.Verify(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Summary_ShouldReturnFile_OnSuccess_ForPdf()
        {
            _mockDocumentWorkflow.Setup(_ => _.GenerateSummaryDocumentAsync(
                It.IsAny<EDocumentType>(), It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());

            var result = await _controller.Summary(EDocumentType.Pdf, "This is some text");

            var fileContentResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("summary.pdf", fileContentResult.FileDownloadName);
            Assert.Equal("application/pdf", fileContentResult.ContentType);
            _mockDocumentWorkflow.Verify(_ => _.GenerateSummaryDocumentAsync(It.IsAny<EDocumentType>(), It.IsAny<string>()), Times.Once);
        }
    }
}