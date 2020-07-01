using System.Threading.Tasks;
using Moq;
using Xunit;
using form_builder.Models;
using form_builder.Enum;
using form_builder.ContentFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using form_builder.Helpers.PageHelpers;
using System.Collections.Generic;
using System;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using form_builder.Builders;
using form_builder.ViewModels;

namespace form_builder_tests.UnitTests.ContentFactory
{
    public class SuccessPageContentFactoryTests
    {
        private readonly SuccessPageFactory _factory;
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();
        private readonly Mock<IHttpContextAccessor> _mockHttpContext = new Mock<IHttpContextAccessor>();
        private readonly Mock<IPageFactory> _mockPageContentFactory = new Mock<IPageFactory>();

        public SuccessPageContentFactoryTests()
        {
            _factory = new SuccessPageFactory(_mockHttpContext.Object, _mockHostingEnv.Object, _mockPageHelper.Object, _mockPageContentFactory.Object);
        }

        [Theory]
        [InlineData("prod")]
        [InlineData("stage")]
        public async Task Build_ShouldThrowException_WhenPageIsNull_AndInvalidEnvironment(string env)
        {
            // Arrange
            var formName = "form-name";
            _mockHttpContext.Setup(_ => _.HttpContext.Request.Host).Returns(new HostString("test"));
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns(env);

            // Act & Assert
            var result = await Assert.ThrowsAsync<Exception>(() => _factory.Build(formName, new FormSchema { BaseURL = "base-test", StartPageSlug = "page-one", Pages = new List<Page>() }, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm));

            Assert.Equal($"SuccessPageContentFactory::Build, No success page configured for form {formName}", result.Message);
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Never);
        }

        [Fact]
        public async Task Build_ShouldReturn_SuccessPageEntity_WithSubmitViewName_WhenNoSuccessPage_Configured()
        {
            // Arrange
            _mockHttpContext.Setup(_ => _.HttpContext.Request.Host).Returns(new HostString("test"));
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("test");

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema {BaseURL = "base-test", StartPageSlug = "page-one", Pages = new List<Page>() }, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            Assert.Equal("Submit", result.ViewName);
        }

        [Fact]
        public async Task Build_ShouldUseGenericPaymentPage_WhenPaymentJourney_And_NoSuccessPageSpecified()
        {
            var callBack = new Page();
            _mockHttpContext.Setup(_ => _.HttpContext.Request.Host).Returns(new HostString("test"));
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("test");
            _mockPageContentFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string,dynamic>, FormSchema, string, List<object>>((a,b,c,d,e) => callBack = a);

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema {BaseURL = "base-test", StartPageSlug = "page-one", Pages = new List<Page>() }, string.Empty, new FormAnswers(), EBehaviourType.SubmitAndPay);

            // Assert
            Assert.Equal("Success", result.ViewName);
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Equal(6, callBack.Elements.Count);
            Assert.Equal(EElementType.H2, callBack.Elements[0].Type);
            Assert.Equal("Thank you for your payment", callBack.Elements[0].Properties.Text);
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
            var callBack = new Page();
            _mockHttpContext.Setup(_ => _.HttpContext.Request.Host).Returns(new HostString("test"));
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("test");
            _mockPageContentFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string,dynamic>, FormSchema, string, List<object>>((a,b,c,d,e) => callBack = a);

            // Act 
            var result = await _factory.Build(string.Empty, new FormSchema {BaseURL = "base-test", StartPageSlug = "page-one", Pages = new List<Page>{ new Page{ PageSlug = "success", Elements = new List<IElement>{ new H2() } } } }, string.Empty, new FormAnswers(), behaviourType);

            // Assert
            Assert.Equal("Success", result.ViewName);
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Single(callBack.Elements);
            Assert.Equal(EElementType.H2, callBack.Elements[0].Type);
        }

        
        [Theory]
        [InlineData(EBehaviourType.SubmitAndPay)]
        [InlineData(EBehaviourType.SubmitForm)]
        public async Task Build_Should_AddDocumentDownloadButton_WhenDocumentDownloadEnabled(EBehaviourType behaviourType)
        {
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
                .WithStartPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .Build();

            _mockHttpContext.Setup(_ => _.HttpContext.Request.Host).Returns(new HostString("test"));
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("test");
            _mockPageContentFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel())
                .Callback<Page, Dictionary<string,dynamic>, FormSchema, string, List<object>>((a,b,c,d,e) => callBack = a);

            // Act 
            var result = await _factory.Build(string.Empty, formSchema, string.Empty, new FormAnswers(), behaviourType);

            // Assert
            Assert.Equal("Success", result.ViewName);
            _mockPageContentFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()), Times.Once);
            Assert.Equal(2, callBack.Elements.Count);
            Assert.Equal(EElementType.DocumentDownload, callBack.Elements[1].Type);
            Assert.Equal($"Download {EDocumentType.Txt} document", callBack.Elements[1].Properties.Label);
        }

        
        [Fact]
        public async Task Build_ShouldReuturn_Correct_StartFormUrl()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.H2)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithStartPageSlug("page-one")
                .WithBaseUrl("base-test")
                .WithPage(page)
                .Build();

            _mockHttpContext.Setup(_ => _.HttpContext.Request.Host).Returns(new HostString("test"));
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("test");
            _mockPageContentFactory.Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string,dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            // Act 
            var result = await _factory.Build(string.Empty, formSchema, string.Empty, new FormAnswers(), EBehaviourType.SubmitForm);

            // Assert
            Assert.Equal($"https://test/{formSchema.BaseURL}/{formSchema.StartPageSlug}", result.StartFormUrl);
        }
    }
}