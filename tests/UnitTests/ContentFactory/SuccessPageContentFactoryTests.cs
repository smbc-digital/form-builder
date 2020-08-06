using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.ContentFactory
{
    public class SuccessPageContentFactoryTests
    {
        private readonly SuccessPageFactory _factory;
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();
        private readonly Mock<IPageFactory> _mockPageContentFactory = new Mock<IPageFactory>();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();

        public SuccessPageContentFactoryTests()
        {
            _factory = new SuccessPageFactory(
                _mockPageHelper.Object,
                _mockPageContentFactory.Object,
                _mockSessionHelper.Object,
                _mockDistributedCache.Object);
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
            _mockPageHelper.Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>())).Returns((Page)null);

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
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string, dynamic>, FormSchema, string, List<object>>((a, b, c, d, e) => callBack = a);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns((Page)null);

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema { BaseURL = "base-test", FirstPageSlug = "page-one", Pages = new List<Page>() }, string.Empty, new FormAnswers(), EBehaviourType.SubmitAndPay);

            // Assert
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Once);
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
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string, dynamic>, FormSchema, string, List<object>>((a, b, c, d, e) => callBack = a);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(Page);

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema { BaseURL = "base-test", FirstPageSlug = "page-one", Pages = new List<Page> { new Page { PageSlug = "success", Elements = new List<IElement> { new H2() } } } }, string.Empty, new FormAnswers(), behaviourType);

            // Assert
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Equal("Success", result.ViewName);
            Assert.Single(callBack.Elements);
            Assert.Equal(EElementType.H2, callBack.Elements[0].Type);
        }

        [Theory]
        [InlineData(EBehaviourType.SubmitAndPay)]
        [InlineData(EBehaviourType.SubmitForm)]
        public async Task Build_Should_AddDocumentDownloadButton_WhenDocumentDownloadEnabled(EBehaviourType behaviourType)
        {
            // Arrange
            var callBack = new Page();

            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("success")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithFirstPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .Build();

            _mockPageContentFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string, dynamic>, FormSchema, string, List<object>>((a, b, c, d, e) => callBack = a);

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns(formSchema.Pages.FirstOrDefault());

            // Act 
            var result = await _factory.Build(string.Empty, formSchema, string.Empty, new FormAnswers(), behaviourType);

            // Assert
            Assert.Equal("Success", result.ViewName);
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Equal(2, callBack.Elements.Count);
            Assert.Equal(EElementType.DocumentDownload, callBack.Elements[1].Type);
            Assert.Equal($"Download {EDocumentType.Txt} document", callBack.Elements[1].Properties.Label);
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
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            _mockPageHelper
                .Setup(_ => _.GetPageWithMatchingRenderConditions(It.IsAny<List<Page>>()))
                .Returns((Page)null);

            // Act 
            var result = await _factory.Build(string.Empty, formSchema, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            Assert.Equal(formSchema.StartPageUrl, result.StartPageUrl);
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

            var formSchema = new FormSchemaBuilder()
                .WithStartPageUrl("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .Build();

            var guid = new Guid();

            // Act
            await _factory.Build(string.Empty, formSchema, guid.ToString(), new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            _mockSessionHelper.Verify(_ => _.RemoveSessionGuid(), Times.Once);
            _mockDistributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == guid.ToString())), Times.Once);
        }
    }
}