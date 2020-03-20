using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class StreetTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();

        public StreetTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task Street_ShouldCallViewRenderWithCorrectPartial_WhenStreetSearch()
        {
            var element = new form_builder.Models.Elements.Street { Properties = new BaseProperty { Text = "text", QuestionId = "street" } };

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("StreetStatus", "Select");
            viewModel.Add("street-streetaddress", "");
            viewModel.Add("street-street", "street");

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
                .Build();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>(), viewModel, page, schema, _mockHostingEnv.Object);

            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "StreetSelect"), It.IsAny<Tuple<ElementViewModel, List<SelectListItem>>>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task Street_ShouldCallViewRenderWithCorrectPartial_WhenStreetSelect()
        {
            //Arrange
            var element = new form_builder.Models.Elements.Street { Properties = new BaseProperty { Text = "text", QuestionId = "street", StreetProvider = "test" } };

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("StreetStatus", "Search");

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
                .Build();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>(), viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "StreetSearch"),It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

    }
}
