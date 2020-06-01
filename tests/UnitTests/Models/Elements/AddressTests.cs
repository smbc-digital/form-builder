using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Elements;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class AddressTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();

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
            viewModel.Add("AddressStatus", "Search");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>(), viewModel, page, schema, _mockHostingEnv.Object);

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

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("AddressStatus", "Select");

            _mockElementHelper.Setup(_ => _.CurrentValue(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()))
                .Returns("SK1 3XE");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>(), viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "AddressSelect"),It.IsAny<form_builder.Models.Elements.Address>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task Address_ShouldGenerateValidUrl_ForAddressSelect()
        {
            //Arrange
            var elementView = new ElementViewModel();
            var addressList = new List<SelectListItem>();
            var callback = new form_builder.Models.Elements.Address();

            _mockElementHelper.Setup(_ => _.CurrentValue(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()))
                .Returns("SK1 3XE");

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Address>(), null))
                .Callback<string, form_builder.Models.Elements.Address, Dictionary<string, object>>((x, y, z) => callback = y);

            var pageSlug = "page-one";
            var baseUrl = "test";

            var element = (form_builder.Models.Elements.Address)new AddressBuilder()
                .WithPropertyText("test")
                .WithQuestionId("address-test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug(pageSlug)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("AddressStatus", "Select");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl(baseUrl)
                .Build();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>(), viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}", callback.ReturnURL);
        }
    }
}
