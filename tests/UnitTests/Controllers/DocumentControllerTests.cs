using Xunit;
using form_builder.Controllers.Document;
using Moq;
using Microsoft.Extensions.Logging;
using form_builder.Services.DocumentService;
using form_builder.Enum;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using form_builder.Exceptions;

namespace form_builder_tests.UnitTests.Controllers
{
    public class DocumentControllerTests
    {
        private DocumentController _controller;
        private Mock<ILogger<DocumentController>> _mockLogger = new Mock<ILogger<DocumentController>>();
        private Mock<IDocumentWorkflow> _mockDocumentWorkflow = new Mock<IDocumentWorkflow>();

        public DocumentControllerTests()
        {
            _controller = new DocumentController(_mockDocumentWorkflow.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Summary_ShouldRedirect_ToError_WhenInvalidIdProvider()
        {
            
            var result = await _controller.Summary(EDocumentType.Txt, Guid.Empty);

            Assert.IsType<RedirectToActionResult>(result);
            _mockDocumentWorkflow.Verify(_ => _.GenerateDocument(It.IsAny<EDocumentContentType>(), It.IsAny<EDocumentType>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Summary_ShouldRedirect_ToExpired_When_ServiceThrows_DocumentExpiredExceptionException()
        {
            _mockDocumentWorkflow.Setup(_ => _.GenerateDocument(It.IsAny<EDocumentContentType>(), It.IsAny<EDocumentType>(), It.IsAny<Guid>()))
                .ThrowsAsync(new DocumentExpiredException("an exception"));
            
            var result = await _controller.Summary(EDocumentType.Txt, Guid.NewGuid());

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Expired", redirectResult.ActionName);
            _mockDocumentWorkflow.Verify(_ => _.GenerateDocument(It.IsAny<EDocumentContentType>(), It.IsAny<EDocumentType>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task Summary_ShouldReturnFile_OnSuccess()
        {
            _mockDocumentWorkflow.Setup(_ => _.GenerateDocument(It.IsAny<EDocumentContentType>(), It.IsAny<EDocumentType>(), It.IsAny<Guid>()))
                .ReturnsAsync(new byte[0]);
            
            var result = await _controller.Summary(EDocumentType.Txt, Guid.NewGuid());

            var fileContentResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("summary.txt", fileContentResult.FileDownloadName);
            Assert.Equal("text/plain", fileContentResult.ContentType);
            _mockDocumentWorkflow.Verify(_ => _.GenerateDocument(It.IsAny<EDocumentContentType>(), It.IsAny<EDocumentType>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}
