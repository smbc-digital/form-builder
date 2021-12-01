using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class AddressTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new ();
        private readonly Mock<IElementHelper> _mockElementHelper = new ();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new ();

        public AddressTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task RenderAsync_ShouldCallViewRenderWithCorrectPartial_WhenAddressSearch()
        {
            //Arrange
            var element = new AddressBuilder()
                .WithPropertyText("test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();
            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                "",
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "AddressSearch"), It.IsAny<Address>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldCallViewRenderWithCorrectPartial_WhenAddressSelect()
        {
            //Arrange
            var element = new AddressBuilder()
                .WithPropertyText("test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {LookUpConstants.SubPathViewModelKey,
                    LookUpConstants.Automatic}
            };

            _mockElementHelper.Setup(_ => _.CurrentValue(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns("SK1 3XE");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                new List<object>());

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "AddressSelect"), It.IsAny<Address>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldGenerateValidUrl_ForAddressSelect()
        {
            //Arrange
            var callback = new Address();

            _mockElementHelper.Setup(_ => _.CurrentValue(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns("SK1 3XE");

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Address>(), null))
                .Callback<string, Address, Dictionary<string, object>>((x, y, z) => callback = y);

            var pageSlug = "page-one";
            var baseUrl = "test";

            var element = new AddressBuilder()
                .WithPropertyText("test")
                .WithQuestionId("address-test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug(pageSlug)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {LookUpConstants.SubPathViewModelKey,
                    LookUpConstants.Automatic}
            };

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl(baseUrl)
                .Build();

            var formAnswers = new FormAnswers();

            //Act
            await element.RenderAsync(_mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                new List<object>());

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}", callback.ReturnURL);
        }

        [Fact]
        public async Task RenderAsync_ShouldThrow_ForAddressSelect_IfResultsIsNull()
        {
            //Arrange
            var element = new AddressBuilder()
                .WithPropertyText("test")
                .WithQuestionId("address-test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {LookUpConstants.SubPathViewModelKey,
                    LookUpConstants.Automatic}
            };

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl("test")
                .Build();

            var formAnswers = new FormAnswers();

            //Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => element.RenderAsync(_mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                formAnswers,
                null));
            

            //Assert
            Assert.Equal($"Address::RenderAsync: retrieved automatic address search results cannot be null", result.Message);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_IsDefined()
        {
            //Arrange
            var label = "Custom label";
            
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithQuestionId("address-test")
                .WithSummaryLabel(label)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal(label, result);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_Is_Not_Defined()
        {
            var pageTitle = "Page Title";
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithQuestionId("address-test")
                .Build();

            //Act
            var result = element.GetLabelText(pageTitle);

            //Assert
            Assert.Equal(pageTitle, result);
        }
    }
}