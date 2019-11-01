using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers;
using form_builder_tests.Builders;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class PageHelperTests
    {
        private readonly PageHelper _pageHelper;
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<ICacheProvider> _mockCacheProvider = new Mock<ICacheProvider>();
        private readonly Mock<IOptions<DisallowedAnswerKeysConfiguration>> _mockDisallowedKeysOptions = new Mock<IOptions<DisallowedAnswerKeysConfiguration>>();
        public PageHelperTests()
        {
            _mockDisallowedKeysOptions.Setup(_ => _.Value).Returns(new DisallowedAnswerKeysConfiguration
            {
                DisallowedAnswerKeys = new []
                {
                    "Guid", "Path"
                }
            });

            _pageHelper = new PageHelper(_mockIViewRender.Object, _mockElementHelper.Object, _mockCacheProvider.Object, _mockDisallowedKeysOptions.Object);
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

            var result = await _pageHelper.GenerateHtml(page, viewModel, schema);

            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "H1"), It.Is<Element>(x => x.Properties.Text == "form-name")));
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
        [InlineData(EElementType.Button)]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial(EElementType type)
        {
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


            var result = await _pageHelper.GenerateHtml(page, viewModel, schema);

            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == type.ToString()), It.IsAny<Element>()), Times.Once);
        }

        [Fact]
        public void SaveAnswers_ShouldCallCacheProvider()
        {
            var guid = Guid.NewGuid();
            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Guid", guid.ToString());
            viewModel.Add("Path", "path");

            _pageHelper.SaveAnswers(viewModel);

            _mockCacheProvider.Verify(_ => _.GetString(It.Is<string>(x => x == guid.ToString())));
            _mockCacheProvider.Verify(_ => _.SetString(It.Is<string>(x => x == guid.ToString()), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Fact]
        public void SaveAnswers_ShouldRemoveCurrentPageData_IfPageKey_AlreadyExists()
        {
            var item1Data = "item1-data";
            var item2Data = "item2-data";

            var callbackCacheProvider = string.Empty;
            var mockData = JsonConvert.SerializeObject(new List<FormAnswers> { new FormAnswers { PageUrl = "path", Answers = new List<Answers> { new Answers { QuestionId = "Item1", Response = "old-answer" }, new Answers { QuestionId = "Item2", Response = "old-answer" } } } });
            _mockCacheProvider.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);


            _mockCacheProvider.Setup(_ => _.SetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string, string, int>((x, y, z) => callbackCacheProvider = y);

            var guid = Guid.NewGuid();
            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Guid", guid.ToString());
            viewModel.Add("Path", "path");
            viewModel.Add("Item1", item1Data);
            viewModel.Add("Item2", item2Data);

            _pageHelper.SaveAnswers(viewModel);

            var callbackModel = JsonConvert.DeserializeObject<List<FormAnswers>>(callbackCacheProvider);

            Assert.Equal("Item1", callbackModel[0].Answers[0].QuestionId);
            Assert.Equal(item1Data, callbackModel[0].Answers[0].Response);

            Assert.Equal("Item2", callbackModel[0].Answers[1].QuestionId);
            Assert.Equal(item2Data, callbackModel[0].Answers[1].Response);
        }

        [Fact]
        public void SaveAnswers_ShouldNotAddKeys_OnDisallowedList()
        {
            var callbackCacheProvider = string.Empty;

            _mockCacheProvider.Setup(_ => _.SetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string, string, int>((x, y, z) => callbackCacheProvider = y);

            var guid = Guid.NewGuid();
            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Guid", guid.ToString());
            viewModel.Add("Path", "path");

            _pageHelper.SaveAnswers(viewModel);

            var callbackModel = JsonConvert.DeserializeObject<List<FormAnswers>>(callbackCacheProvider);

            Assert.Empty(callbackModel[0].Answers);
        }

        [Fact]
        public void SaveAnswers_AddAnswersInViewModel()
        {
            var callbackCacheProvider = string.Empty;
            var item1Data = "item1-data";
            var item2Data = "item2-data";

            _mockCacheProvider.Setup(_ => _.SetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string, string, int>((x, y, z) => callbackCacheProvider = y);

            var guid = Guid.NewGuid();
            var viewModel = new Dictionary<string, string>();
            viewModel.Add("Guid", guid.ToString());
            viewModel.Add("Path", "path");
            viewModel.Add("Item1", item1Data);
            viewModel.Add("Item2", item2Data);

            _pageHelper.SaveAnswers(viewModel);

            var callbackModel = JsonConvert.DeserializeObject<List<FormAnswers>>(callbackCacheProvider);

            Assert.Equal("Item1", callbackModel[0].Answers[0].QuestionId);
            Assert.Equal(item1Data, callbackModel[0].Answers[0].Response);

            Assert.Equal("Item2", callbackModel[0].Answers[1].QuestionId);
            Assert.Equal(item2Data, callbackModel[0].Answers[1].Response);
        }
    }
}
