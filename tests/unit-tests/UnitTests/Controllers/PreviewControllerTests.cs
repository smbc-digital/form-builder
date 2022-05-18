using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Controllers;
using form_builder.Models;
using form_builder.Services.PageService.Entities;
using form_builder.Services.PreviewService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Controllers
{
    public class PreviewControllerTests
    {
        private readonly PreviewController _controller;
        private readonly Mock<IPreviewService> _previewService = new();

        public PreviewControllerTests()
        {
            _controller = new PreviewController(_previewService.Object);
        }

        [Fact]
        public async Task Index_Should_CallService_And_Return_View()
        {
            var result = await _controller.Index();

            _previewService.Verify(_ => _.GetPreviewPage(), Times.Once);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task IndexPost_Should_CallService_And_Redirect_OnValid_Request()
        {
            var previewKey = "key-123456";
            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            _previewService
                .Setup(_ => _.VerifyPreviewRequest(It.IsAny<List<CustomFormFile>>()))
                .ReturnsAsync(new ProcessPreviewRequestEntity { PreviewFormKey = previewKey, Page = page, UseGeneratedViewModel = false });

            var result = await _controller.IndexPost(new List<CustomFormFile>());

            _previewService.Verify(_ => _.VerifyPreviewRequest(It.IsAny<List<CustomFormFile>>()), Times.Once);
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(previewKey, redirectResult.Url);
        }

        [Fact]
        public async Task IndexPost_ShouldReturn_View_IfUseGeneratedViewModel_IsTrue()
        {
            var previewKey = "key-123456";
            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            _previewService
                .Setup(_ => _.VerifyPreviewRequest(It.IsAny<List<CustomFormFile>>()))
                .ReturnsAsync(new ProcessPreviewRequestEntity { PreviewFormKey = previewKey, Page = page, UseGeneratedViewModel = true });

            var result = await _controller.IndexPost(new List<CustomFormFile>());

            _previewService.Verify(_ => _.VerifyPreviewRequest(It.IsAny<List<CustomFormFile>>()), Times.Once);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Exit_ShouldCallService_AndReturnView()
        {
            var result = _controller.Exit();

            _previewService.Verify(_ => _.ExitPreviewMode(), Times.Once);
            Assert.IsType<ViewResult>(result);
        }
    }
}
