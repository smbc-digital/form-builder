using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
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
    public class OrganisationTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();

        public OrganisationTests(){
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithListOf_SelectListItem_FromOrgSearchResults()
        {
            var elementView = new ElementViewModel();
            var orgSearchList = new List<SelectListItem>();
            var callback = new form_builder.Models.Elements.Organisation();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, form_builder.Models.Elements.Organisation, Dictionary<string, object>>((x, y, z) => callback = y);

            //Arrange
            var organisationElement = (form_builder.Models.Elements.Organisation)new ElementBuilder()
                .WithType(EElementType.Organisation)
                .WithStreetProvider("Fake")
                .Build();

            var page = new PageBuilder()
                .WithElement(organisationElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("OrganisationStatus", "Select");
            viewModel.Add(organisationElement.OrganisationSearchQuestionId, "test org");

            //Act
            var result = await organisationElement.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>(), viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "OrganisationSelect"),It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Single(callback.Items);
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithListOf_SelectListItem_GeneratedFromOrgSearchResults()
        {
            var elementView = new ElementViewModel();
            var orgSearchList = new List<SelectListItem>();
            var callback = new form_builder.Models.Elements.Organisation();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, form_builder.Models.Elements.Organisation, Dictionary<string, object>>((x, y, z) => callback = y);

            //Arrange
            var organisationElement = (form_builder.Models.Elements.Organisation)new ElementBuilder()
                .WithType(EElementType.Organisation)
                .WithStreetProvider("Fake")
                .Build();

            var page = new PageBuilder()
                .WithElement(organisationElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("OrganisationStatus", "Select");
            viewModel.Add(organisationElement.OrganisationSearchQuestionId, "test org");

            //Act
            var result = await organisationElement.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>{ new OrganisationSearchResult{ Reference = "123455", Name = "name123" } }, viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "OrganisationSelect"),It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Equal(2, callback.Items.Count);
        }
    }
}
