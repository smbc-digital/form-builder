using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class PageHelperTests
    {
        private readonly PageHelper _pageHelper;
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IOptions<DisallowedAnswerKeysConfiguration>> _mockDisallowedKeysOptions = new Mock<IOptions<DisallowedAnswerKeysConfiguration>>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();

        public PageHelperTests()
        {
            _mockDisallowedKeysOptions.Setup(_ => _.Value).Returns(new DisallowedAnswerKeysConfiguration
            {
                DisallowedAnswerKeys = new []
                {
                    "Guid", "Path"
                }
            });

            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");

            _pageHelper = new PageHelper(_mockIViewRender.Object, _mockElementHelper.Object, _mockDistributedCache.Object, _mockDisallowedKeysOptions.Object, _mockHostingEnv.Object);
        }

        [Fact]
        public async Task GenerateHtml_ShouldRenderH1Element_WithBaseformName()
        {
            var page = new PageBuilder()
                .Build();
            var viewModel = new Dictionary<string, string>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "H1"), It.Is<Element>(x => x.Properties.Text == "form-name"), It.IsAny<Dictionary<string, object>>()));
        }

        [Theory]
        [InlineData(EElementType.H2)]
        [InlineData(EElementType.H3)]
        [InlineData(EElementType.H4)]
        [InlineData(EElementType.H5)]
        [InlineData(EElementType.H6)]
        [InlineData(EElementType.P)]
        [InlineData(EElementType.Span)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.Checkbox)]
        [InlineData(EElementType.Select)]
        [InlineData(EElementType.Button)]
        [InlineData(EElementType.DateInput)]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial(EElementType type)
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithPropertyText("text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, string>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == type.ToString()), It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSelect()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithPropertyText("text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("AddressStatus", "Select");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Address"), It.IsAny<Element>(), null), Times.Once);
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

            var addressElement = new form_builder.Models.Elements.Address { Properties = new Property { Text = "text" } };

            var page = new PageBuilder()
                .WithElement(addressElement)
                .WithPageSlug(pageSlug)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("AddressStatus", "Select");
            
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl(baseUrl)
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}/address", callback.Item1.ReturnURL);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSearch()
        {
            //Arrange
            var addressElement = new form_builder.Models.Elements.Address { Properties = new Property { Text = "text" } };

            var page = new PageBuilder()
                .WithElement(addressElement)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("AddressStatus", "Search");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "AddressSearch"), It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenStreetSelect()
        {
            //Arrange
            var element = new form_builder.Models.Elements.Street{ Properties =  new Property { QuestionId = "street", StreetProvider = "FakeStreet" } };
                

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("StreetStatus", "Select");

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "StreetSelect"), It.IsAny<Tuple<ElementViewModel, List<AddressSearchResult>>>(), null), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenStreetSearch()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Street)
                .WithPropertyText("text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("StreetStatus", "Search");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Street"), It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        //[Fact]
        //public async Task GenerateHtml_ShouldGenerateValidUrl_ForStreetSelect()
        //{
        //    //Arrange

        //    var elementView = new Element();
        //    var streetList = new List<StockportGovUK.NetStandard.Models.Models.Verint.Street>();
        //    var callback = new Tuple<Element, List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>(elementView, streetList);

        //    _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Tuple<Element, List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>>(), null))
        //        .Callback<string, Tuple<Element, List<StockportGovUK.NetStandard.Models.Models.Verint.Street>>, Dictionary<string, object>>((x, y, z) => callback = y);

        //    var pageSlug = "page-one";
        //    var baseUrl = "test";
        //    var element = new ElementBuilder()
        //        .WithType(EElementType.Street)
        //        .WithPropertyText("text")
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithPageSlug(pageSlug)
        //        .Build();

        //    var viewModel = new Dictionary<string, string>();
        //    viewModel.Add("StreetStatus", "Select");

        //    var schema = new FormSchemaBuilder()
        //        .WithName("form-name")
        //        .WithBaseUrl(baseUrl)
        //        .Build();

        //    //Act
        //    var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

        //    //Assert
        //    Assert.Equal($"/{baseUrl}/{pageSlug}/street", callback.Item1.);
        //}

        [Theory]
        [InlineData(EElementType.OL)]
        [InlineData(EElementType.UL)]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartialForList(EElementType type)
        {
            //Arrange
            List<string> listItems = new List<string> { "item 1", "item 2", "item 3" };

            var element = new ElementBuilder()
                .WithType(type)
                .WithListItems(listItems)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, string>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == type.ToString()), It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }


        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartialForImg()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Img)
                .WithAltText("alt text")
                .WithSource("source")
                .Build();

            var page = new PageBuilder()
               .WithElement(element)
               .Build();

            var viewModel = new Dictionary<string, string>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == EElementType.Img.ToString()), It.IsAny<Element>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public void SaveAnswers_ShouldCallCacheProvider()
        {
            var guid = Guid.NewGuid();
            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Path", "path");

            var mockData = JsonConvert.SerializeObject(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _pageHelper.SaveAnswers(viewModel, guid.ToString());

            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(x => x == guid.ToString())));
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x == guid.ToString()), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void SaveAnswers_ShouldRemoveCurrentPageData_IfPageKey_AlreadyExists()
        {
            var item1Data = "item1-data";
            var item2Data = "item2-data";

            var callbackCacheProvider = string.Empty;

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        PageSlug = "path",
                        Answers = new List<Answers>
                        {
                             new Answers { QuestionId = "Item1", Response = "old-answer" },
                             new Answers { QuestionId = "Item2", Response = "old-answer" }
                        }
                    }
                }
            };

            var mockData = JsonConvert.SerializeObject(formAnswers);

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache.Setup(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Path", "path");
            viewModel.Add("Item1", item1Data);
            viewModel.Add("Item2", item2Data);

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString());

            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            Assert.Equal("Item1", callbackModel.Pages[0].Answers[0].QuestionId);
            Assert.Equal(item1Data, callbackModel.Pages[0].Answers[0].Response);

            Assert.Equal("Item2", callbackModel.Pages[0].Answers[1].QuestionId);
            Assert.Equal(item2Data, callbackModel.Pages[0].Answers[1].Response);
        }

        [Fact]
        public void SaveAnswers_ShouldNotAddKeys_OnDisallowedList()
        {
            var callbackCacheProvider = string.Empty;

            _mockDistributedCache.Setup(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var mockData = JsonConvert.SerializeObject(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Path", "path");

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString());

            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            Assert.Empty(callbackModel.Pages[0].Answers);
        }

        [Fact]
        public void SaveAnswers_AddAnswersInViewModel()
        {
            var callbackCacheProvider = string.Empty;
            var item1Data = "item1-data";
            var item2Data = "item2-data";
            var mockData = JsonConvert.SerializeObject(new FormAnswers { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache.Setup(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Path", "path");
            viewModel.Add("Item1", item1Data);
            viewModel.Add("Item2", item2Data);

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString());

            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            Assert.Equal("Item1", callbackModel.Pages[0].Answers[0].QuestionId);
            Assert.Equal(item1Data, callbackModel.Pages[0].Answers[0].Response);

            Assert.Equal("Item2", callbackModel.Pages[0].Answers[1].QuestionId);
            Assert.Equal(item2Data, callbackModel.Pages[0].Answers[1].Response);
        }
    }
}
