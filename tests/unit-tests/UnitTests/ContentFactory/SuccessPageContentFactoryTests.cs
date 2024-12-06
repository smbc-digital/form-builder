using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.ContentFactory.SuccessPageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.ContentFactory
{
    public class SuccessPageContentFactoryTests
    {
        private readonly SuccessPageFactory _factory;
        private readonly Mock<IPageHelper> _mockPageHelper = new();
        private readonly Mock<IPageFactory> _mockPageContentFactory = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment = new();
        private readonly Mock<IOptions<PreviewModeConfiguration>> _mockPreviewModeConfiguration = new();

        private readonly Mock<ILogger<SuccessPageFactory>> _logger = new();

        public SuccessPageContentFactoryTests()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration());

            _factory = new SuccessPageFactory(
                _mockPageHelper.Object,
                _mockPageContentFactory.Object,
                _mockSessionHelper.Object,
                _mockDistributedCache.Object,
                _mockPreviewModeConfiguration.Object,
                _mockWebHostEnvironment.Object,
                _logger.Object);

            _mockWebHostEnvironment.Setup(_ => _.EnvironmentName).Returns("local");
        }

        private static readonly Page Page = new PageBuilder()
            .WithPageSlug("success")
            .WithElement(new Element
            {
                Type = EElementType.H2
            })
            .Build();

        [Fact]
        public async Task Build_ShouldReturn_SuccessPageEntity_WithSubmitViewName_WhenNoSuccessPage_Configured()
        {
            // Arrange
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>())).Returns((Page)null);

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema { BaseURL = "base-test", FirstPageSlug = "page-one", Pages = new List<Page>() }, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            Assert.Equal("Submit", result.ViewName);
        }

        [Fact]
        public async Task Build_ShouldUseGenericPaymentPage_WhenPaymentJourney_And_NoSuccessPageSpecified()
        {
            // Arrange
            var callBack = new Page();
            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string, dynamic>, FormSchema, string, FormAnswers, List<object>>((a, b, c, d, e, f) => callBack = a);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns((Page)null);

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema { BaseURL = "base-test", FirstPageSlug = "page-one", Pages = new List<Page>() }, string.Empty, new FormAnswers(), EBehaviourType.SubmitAndPay);

            // Assert
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Equal("Success", result.ViewName);
            Assert.Equal("Thank you for your payment", callBack.Elements[0].Properties.Text);
            Assert.Equal(6, callBack.Elements.Count);
            Assert.Equal(EElementType.H2, callBack.Elements[0].Type);
            Assert.Equal(EElementType.P, callBack.Elements[1].Type);
            Assert.Equal(EElementType.P, callBack.Elements[2].Type);
            Assert.Equal(EElementType.H2, callBack.Elements[3].Type);
            Assert.Equal(EElementType.P, callBack.Elements[4].Type);
            Assert.Equal(EElementType.P, callBack.Elements[5].Type);
        }

        [Theory]
        [InlineData(EBehaviourType.SubmitAndPay)]
        [InlineData(EBehaviourType.SubmitForm)]
        public async Task Build_ShouldUseSpecifiedSuccessPage(EBehaviourType behaviourType)
        {
            // Arrange
            var callBack = new Page();
            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string, dynamic>, FormSchema, string, FormAnswers, List<object>>((a, b, c, d, e, f) => callBack = a);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(Page);

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema { BaseURL = "base-test", FirstPageSlug = "page-one", Pages = new List<Page> { new Page { PageSlug = "success", Elements = new List<IElement> { new H2() } } } }, string.Empty, new FormAnswers(), behaviourType);

            // Assert
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Equal("Success", result.ViewName);
            Assert.Single(callBack.Elements);
            Assert.Equal(EElementType.H2, callBack.Elements[0].Type);
        }

        [Fact]
        public async Task Build_Should_Set_IsInPreviewMode_ToTrue()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithFirstPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .Build();

            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel { IsInPreviewMode = true });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns(formSchema.Pages.FirstOrDefault());

            // Act 
            var result = await _factory.Build(string.Empty, formSchema, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            Assert.True(result.IsInPreviewMode);
        }

        [Fact]
        public async Task Build_ShouldReturn_Correct_StartPageUrl()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithStartPageUrl("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .Build();

            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns((Page)null);

            // Act 
            var result = await _factory.Build(string.Empty, formSchema, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            Assert.Equal(formSchema.StartPageUrl, result.StartPageUrl);
        }

        [Fact]
        public async Task Build_ShouldReturn_IsInPreviewMode_True_When_PreviewMode_Enabled_And_Matching_BaseUrl_With_Generic_SuccessPage()
        {
            _mockPreviewModeConfiguration
                .Setup(_ => _.Value)
                .Returns(new PreviewModeConfiguration { IsEnabled = true });

            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithStartPageUrl("page-one")
                .WithBaseUrl(PreviewConstants.PREVIEW_MODE_PREFIX)
                .WithPage(page)
                .Build();

            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel { IsInPreviewMode = true });

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>(), It.IsAny<string>()))
                .Returns((Page)null);

            // Act 
            var result = await _factory.Build(string.Empty, formSchema, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            Assert.True(result.IsInPreviewMode);
        }

        [Fact]
        public async Task Build_ShouldDeleteCacheEntry()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var successPage = new PageBuilder()
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithStartPageUrl("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .WithPage(successPage)
                .Build();

            var guid = new Guid();

            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(),
                    It.IsAny<string>(), It.IsAny<FormAnswers>(), null))
                .ReturnsAsync(new FormBuilderViewModel());

            // Act
            await _factory.Build(string.Empty, formSchema, guid.ToString(), new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            _mockDistributedCache.Verify(_ => _.Remove(It.Is<string>(x => x.Equals(guid.ToString()))), Times.Once);
        }

        [Fact]
        public async Task BuildBooking_ShouldReturn_GenericBookingSuccess_Page()
        {
            // Arrange
            var pageCallback = new Page();
            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string, dynamic>, FormSchema, string, FormAnswers, List<object>>((a, b, c, d, e, f) => pageCallback = a);

            var formSchema = new FormSchemaBuilder()
                .WithStartPageUrl("page-one")
                .WithBaseUrl("base-form")
                .Build();

            var guid = new Guid();

            // Act
            var result = await _factory.BuildBooking("base-form", formSchema, guid.ToString(), new FormAnswers());

            // Assert
            Assert.IsType<SuccessPageEntity>(result);
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Single(pageCallback.Elements);
            Assert.Equal("booking-cancel-success", pageCallback.PageSlug);
            Assert.Equal("You've successfully cancelled your appointment", pageCallback.BannerTitle);
            Assert.Equal("We've received your cancellation request", pageCallback.LeadingParagraph);
            Assert.Equal("Success", pageCallback.Title);
            Assert.True(pageCallback.HideTitle);
            Assert.False(result.IsInPreviewMode);
        }
    }
}