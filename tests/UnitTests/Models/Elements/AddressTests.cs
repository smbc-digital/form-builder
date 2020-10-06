using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class AddressTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        public AddressTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSearch()
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
            var result = await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                "",
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                _mockHttpContextAccessor.Object,
                formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "AddressSearch"), It.IsAny<form_builder.Models.Elements.Address>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSelect()
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

            _mockElementHelper.Setup(_ => _.CurrentValue(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()))
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
                _mockHttpContextAccessor.Object,
                formAnswers,
                new List<object>());

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "AddressSelect"),It.IsAny<form_builder.Models.Elements.Address>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task Address_ShouldGenerateValidUrl_ForAddressSelect()
        {
            //Arrange
            var callback = new Address();

            _mockElementHelper.Setup(_ => _.CurrentValue(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()))
                .Returns("SK1 3XE");

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Address>(), null))
                .Callback<string, form_builder.Models.Elements.Address, Dictionary<string, object>>((x, y, z) => callback = y);

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
            var result = await element.RenderAsync(_mockIViewRender.Object,
                _mockElementHelper.Object,
                string.Empty,
                viewModel,
                page,
                schema,
                _mockHostingEnv.Object,
                _mockHttpContextAccessor.Object,
                formAnswers,
                new List<object>());

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}", callback.ReturnURL);
        }
    }
}