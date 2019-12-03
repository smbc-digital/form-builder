using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
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
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IOptions<DisallowedAnswerKeysConfiguration>> _mockDisallowedKeysOptions = new Mock<IOptions<DisallowedAnswerKeysConfiguration>>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();

        public AddressTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task Address_ShouldCallViewRenderWithCorrectPartial_WhenStreetSelect()
        {
            var element = new ElementBuilder()
                  .WithType(EElementType.Street)
                  .WithQuestionId("street")
                  .WithPropertyText("text")
                  .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("StreetStatus", "Select");

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
                .Build();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<StockportGovUK.NetStandard.Models.Models.Verint.Street>(), viewModel, page, schema, _mockHostingEnv.Object);


            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Street"), It.IsAny<Element>(), null), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldGenerateValidUrl_ForAddressSelect()
        {
            //Arrange

            var elementView = new ElementViewModel();
            var addressList = new List<AddressSearchResult>();
            var callback = new Tuple<ElementViewModel, List<AddressSearchResult>>(elementView, addressList);

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Tuple<ElementViewModel, List<AddressSearchResult>>>(), null))
                .Callback<string, Tuple<ElementViewModel, List<AddressSearchResult>>, Dictionary<string, object>>((x, y, z) => callback = y);

            var pageSlug = "page-one";
            var baseUrl = "test";
            var element = new form_builder.Models.Elements.Address { Properties = new Property { QuestionId = "address-test" } };

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug(pageSlug)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("AddressStatus", "Select");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl(baseUrl)
                .Build();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<StockportGovUK.NetStandard.Models.Models.Verint.Street>(), viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}/address", callback.Item1.ReturnURL);
        }
    }
}
