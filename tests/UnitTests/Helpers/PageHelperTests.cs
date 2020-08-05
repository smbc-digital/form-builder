using form_builder.Builders;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.FileUploadService;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Models.Properties.ActionProperties;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class PageHelperTests
    {
        private readonly PageHelper _pageHelper;
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IOptions<DisallowedAnswerKeysConfiguration>> _mockDisallowedKeysOptions =
            new Mock<IOptions<DisallowedAnswerKeysConfiguration>>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();
        private readonly Mock<ICache> _mockCache = new Mock<ICache>();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationSettings
            = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();
        private readonly Mock<IEnumerable<IPaymentProvider>> _mockPaymentProvider =
            new Mock<IEnumerable<IPaymentProvider>>();
        private readonly Mock<IPaymentProvider> _paymentProvider = new Mock<IPaymentProvider>();
        private readonly Mock<IFileUploadService> _mockFileUploadService = new Mock<IFileUploadService>();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();

        public PageHelperTests()
        {
            _mockDisallowedKeysOptions.Setup(_ => _.Value).Returns(new DisallowedAnswerKeysConfiguration
            {
                DisallowedAnswerKeys = new[]
                {
                    "Guid", "Path"
                }
            });

            _mockCache.Setup(_ =>
                    _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(),
                        It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = "test-form",
                        PaymentProvider = "testProvider"
                    },
                    new PaymentInformation
                    {
                        FormName = "test-form-with-invorrect-provider",
                        PaymentProvider = "invalidProvider"
                    }
                });

            _mockDistributedCacheExpirationSettings.Setup(_ => _.Value).Returns(
                new DistributedCacheExpirationConfiguration
                {
                    UserData = 30,
                    PaymentConfiguration = 5,
                    FileUpload = 60
                });

            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");

            _paymentProvider.Setup(_ => _.ProviderName).Returns("testProvider");
            var paymentProviderItems = new List<IPaymentProvider> { _paymentProvider.Object };
            _mockPaymentProvider.Setup(m => m.GetEnumerator()).Returns(() => paymentProviderItems.GetEnumerator());
            _pageHelper = new PageHelper(_mockIViewRender.Object, _mockElementHelper.Object,
                _mockDistributedCache.Object, _mockDisallowedKeysOptions.Object, _mockHostingEnv.Object,
                _mockCache.Object, _mockDistributedCacheExpirationSettings.Object, _mockPaymentProvider.Object,
                _mockFileUploadService.Object, _mockSessionHelper.Object);
        }

        [Fact]
        public async Task GenerateHtml_ShouldRenderH1Element_WithBaseformName()
        {
            var page = new PageBuilder()
                .WithPageTitle("Page title")
                .Build();
            var viewModel = new Dictionary<string, dynamic>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "H1"),
                It.Is<Element>(x => x.Properties.Text == "Page title"), It.IsAny<Dictionary<string, object>>()));
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

            var viewModel = new Dictionary<string, dynamic>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == type.ToString()), It.IsAny<Element>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSelect()
        {
            //Arrange
            _mockElementHelper.Setup(_ => _.CurrentValue<string>(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("SK1 3XE");

            var element = (form_builder.Models.Elements.Address)new ElementBuilder()
                .WithType(EElementType.Address)
                .WithPropertyText("text")
                .WithQuestionId("address-test")
                .WithValue("SK1 3XE")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(LookUpConstants.SubPathViewModelKey, LookUpConstants.Automatic);

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, new List<object>());

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == "AddressSelect"),
                    It.IsAny<form_builder.Models.Elements.Address>(), null), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldGenerateValidUrl_ForAddressSelect()
        {
            //Arrange
            var elementView = new ElementViewModel();
            var addressList = new List<SelectListItem>();
            var callback = new form_builder.Models.Elements.Address();

            _mockIViewRender.Setup(_ =>
                    _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Address>(), null))
                .Callback<string, form_builder.Models.Elements.Address, Dictionary<string, object>>((x, y, z) =>
                    callback = y);

            _mockElementHelper.Setup(_ => _.CurrentValue<string>(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("SK1 3XE");

            var pageSlug = "page-one";
            var baseUrl = "test";

            var addressElement = new form_builder.Models.Elements.Address
            { Properties = new BaseProperty { Text = "text" } };

            var page = new PageBuilder()
                .WithElement(addressElement)
                .WithPageSlug(pageSlug)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(LookUpConstants.SubPathViewModelKey, LookUpConstants.Automatic);

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl(baseUrl)
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, new List<object>());

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}", callback.ReturnURL);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSearch()
        {
            //Arrange
            var addressElement = new form_builder.Models.Elements.Address
            { Properties = new BaseProperty { Text = "text" } };

            var page = new PageBuilder()
                .WithElement(addressElement)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("subPath", "");

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == "AddressSearch"),
                    It.IsAny<form_builder.Models.Elements.Address>(), It.IsAny<Dictionary<string, object>>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenStreetSelect()
        {
            //Arrange
            var element = new form_builder.Models.Elements.Street
            { Properties = new BaseProperty { QuestionId = "street", StreetProvider = "test", Text = "test" } };

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(LookUpConstants.SubPathViewModelKey, LookUpConstants.Automatic);
            viewModel.Add("street-streetaddress", string.Empty);
            viewModel.Add("street-street", "street");

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == "StreetSelect"),
                    It.IsAny<form_builder.Models.Elements.Street>(), null), Times.Once);
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

            var viewModel = new Dictionary<string, dynamic>();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == "StreetSearch"), It.IsAny<Element>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldGenerateValidUrl_ForStreetSelect()
        {
            //Arrange

            var elementView = new ElementViewModel();
            var streetList = new List<SelectListItem>();
            var callback = new form_builder.Models.Elements.Street();

            _mockIViewRender.Setup(_ =>
                    _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Street>(), null))
                .Callback<string, form_builder.Models.Elements.Street, Dictionary<string, object>>((x, y, z) =>
                    callback = y);

            var pageSlug = "page-one";
            var baseUrl = "test";
            var element = new form_builder.Models.Elements.Street { Properties = new BaseProperty { Text = "test" } };

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug(pageSlug)
                .Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(LookUpConstants.SubPathViewModelKey, LookUpConstants.Automatic);
            viewModel.Add("-streetaddress", string.Empty);
            viewModel.Add("-street", "street");
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl(baseUrl)
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}", callback.ReturnURL);
        }

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

            var viewModel = new Dictionary<string, dynamic>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == type.ToString()), It.IsAny<Element>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
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

            var viewModel = new Dictionary<string, dynamic>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, "");

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == EElementType.Img.ToString()), It.IsAny<Element>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public void SaveAnswers_ShouldCallCacheProvider()
        {
            var guid = Guid.NewGuid();
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "path");

            var mockData = JsonConvert.SerializeObject(new FormAnswers
            { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _pageHelper.SaveAnswers(viewModel, guid.ToString(), "formName", null, true);

            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(x => x == guid.ToString())));
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x == guid.ToString()),
                It.IsAny<string>(), It.IsAny<CancellationToken>()));
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
                            new Answers {QuestionId = "Item1", Response = "old-answer"},
                            new Answers {QuestionId = "Item2", Response = "old-answer"}
                        }
                    }
                }
            };

            var mockData = JsonConvert.SerializeObject(formAnswers);

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "path");
            viewModel.Add("Item1", item1Data);
            viewModel.Add("Item2", item2Data);

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);

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

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var mockData = JsonConvert.SerializeObject(new FormAnswers
            { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "path");

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);

            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            Assert.Empty(callbackModel.Pages[0].Answers);
        }

        [Fact]
        public void SaveAnswers_AddAnswersInViewModel()
        {
            var callbackCacheProvider = string.Empty;
            var item1Data = "item1-data";
            var item2Data = "item2-data";
            var mockData = JsonConvert.SerializeObject(new FormAnswers
            { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "path");
            viewModel.Add("Item1", item1Data);
            viewModel.Add("Item2", item2Data);

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);

            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            Assert.Equal("Item1", callbackModel.Pages[0].Answers[0].QuestionId);
            Assert.Equal(item1Data, callbackModel.Pages[0].Answers[0].Response);

            Assert.Equal("Item2", callbackModel.Pages[0].Answers[1].QuestionId);
            Assert.Equal(item2Data, callbackModel.Pages[0].Answers[1].Response);
        }

        [Fact]
        public void SaveAnswers_ShouldSaveFileUpload_WithinDistributedCache_OnSeperateKey()
        {
            var questionId = "fileUpload_testFileQuestionId";
            var fileContent = "abc";
            var fileName = "fileName.txt";

            var collection = new List<CustomFormFile>();
            var fileMock = new CustomFormFile(fileContent, questionId, 0, fileName);
            collection.Add(fileMock);

            var allTheAnswers = new FormAnswers { Path = "page-one", Pages = new List<PageAnswers>() };
            var mockData = JsonConvert.SerializeObject(allTheAnswers);

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "path");
            viewModel.Add(questionId, new DocumentModel { Content = fileContent });

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", collection, true);

            _mockDistributedCache.Verify(
                _ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public void SaveAnswers_ShouldSaveFilUploadReference_WithinFormAnswers_InDistributedCache()
        {
            var callbackCacheProvider = string.Empty;
            var questionId = "fileUpload_testFileQuestionId";
            var fileContent = "abc";
            var fileName = "fileName.txt";

            var collection = new List<CustomFormFile>();
            var fileMock = new CustomFormFile(fileContent, questionId, 0, fileName);
            collection.Add(fileMock);

            var allTheAnswers = new List<Answers>();
            allTheAnswers.Add(new Answers
            {
                QuestionId = questionId,
                Response = new FileUploadModel
                {
                    Key = questionId,
                    TrustedOriginalFileName = $"file-{questionId}",
                    UntrustedOriginalFileName = fileName
                }
            });

            FormAnswers form = new FormAnswers();
            form.FormName = "testpage";
            form.Path = "page-one";
            form.Pages = new List<PageAnswers>();

            PageAnswers page = new PageAnswers
            {
                Answers = allTheAnswers,
                PageSlug = "pageone"
            };
            form.Pages.Add(page);

            var mockData = JsonConvert.SerializeObject(form);

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "path");
            viewModel.Add(questionId, new DocumentModel { Content = fileContent });

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", collection, true);

            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            Assert.Equal(questionId, callbackModel.Pages[0].Answers[0].QuestionId);
            var fileUploadModel =
                JsonConvert.DeserializeObject<FileUploadModel>(callbackModel.Pages[0].Answers[0].Response.ToString());
            Assert.Equal(questionId, fileUploadModel.Key);
        }

        [Fact]
        public void
        SaveAnswers_ShouldReplaceFilUploadReference_WithinFormAnswers_IfAnswerAlreadyExists_InDistributedCache()
        {
            var callbackCacheProvider = string.Empty;
            var questionId = "fileUpload_testFileQuestionId";
            var fileName = "replace-me.txt";
            var fileContent = "abc";

            var collection = new List<CustomFormFile>();
            var fileMock = new CustomFormFile(fileContent, questionId, 0, fileName);
            collection.Add(fileMock);

            var allTheAnswers = new List<Answers>();
            allTheAnswers.Add(new Answers
            {
                QuestionId = questionId,
                Response = new FileUploadModel
                {
                    Key = questionId,
                    TrustedOriginalFileName = $"file-{questionId}",
                    UntrustedOriginalFileName = fileName
                }
            });

            FormAnswers form = new FormAnswers();
            form.FormName = "testpage";
            form.Path = "pageone";
            form.Pages = new List<PageAnswers>();

            PageAnswers page = new PageAnswers
            {
                Answers = allTheAnswers,
                PageSlug = "pageone"
            };
            form.Pages.Add(page);

            var mockData = JsonConvert.SerializeObject(form);

            _mockFileUploadService.Setup(service => service.SaveFormFileAnswers(It.IsAny<List<Answers>>(),
                It.IsAny<IEnumerable<CustomFormFile>>())).Returns(allTheAnswers);

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "page-one");
            viewModel.Add(questionId, new DocumentModel { Content = fileContent });

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", collection, true);

            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            Assert.Equal(questionId, callbackModel.Pages[0].Answers[0].QuestionId);
            FileUploadModel fileUploadModel =
                JsonConvert.DeserializeObject<FileUploadModel>(callbackModel.Pages[0].Answers[0].Response.ToString());
            Assert.Equal(fileName, fileUploadModel.UntrustedOriginalFileName);
        }

        [Fact]
        public void SaveAnswers_ShouldNotCallDistributedCache_ForFileUpload_WhenNoFile()
        {
            var fileMock = new Mock<IFormFile>();

            var mockData = JsonConvert.SerializeObject(new FormAnswers
            { Path = "page-one", Pages = new List<PageAnswers>() });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("Path", "path");

            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);

            _mockDistributedCache.Verify(
                _ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()), Times.Never);
            _mockDistributedCache.Verify(
                _ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void DuplicateIDs_ShouldThrowException_IfDuplicateQuestionIDsInJSON()
        {
            // Arrange
            var element1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox1")
                .WithLabel("First name")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox2")
                .WithLabel("Middle name")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox1")
                .WithLabel("First name")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .Build();

            List<Page> pages = new List<Page>();
            pages.Add(page1);
            pages.Add(page2);

            // Act
            Assert.Throws<ApplicationException>(() => _pageHelper.HasDuplicateQuestionIDs(pages, "form"));
        }

        [Fact]
        public void DuplicateIDs_ShouldNotThrowException_IfNoDuplicateQuestionIDsInJSON()
        {
            // Arrange
            var element1 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox1")
                .WithLabel("First name")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox2")
                .WithLabel("Middle name")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("texbox3")
                .WithLabel("First name")
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .Build();

            List<Page> pages = new List<Page>();
            pages.Add(page1);
            pages.Add(page2);

            // Act
            _pageHelper.HasDuplicateQuestionIDs(pages, "form");
        }

        [Theory]
        [InlineData("invalid-questionId", "")]
        [InlineData("question4", "")]
        [InlineData("question£", "")]
        [InlineData("que!stion", "")]
        [InlineData("quest%ion", "")]
        [InlineData("question.", "")]
        [InlineData(".question", "")]
        [InlineData("", "invalid-tagretMapping")]
        [InlineData("", "tagret4")]
        [InlineData("", "target$")]
        [InlineData("", "target.")]
        [InlineData("", ".target")]
        public void
        CheckForInvalidQuestionOrTargetMappingValue_ShouldThrowExceptionWhen_InvalidQuestionId_OrTargetMapping(
                string questionId, string targetMapping)
        {
            var pages = new List<Page>();

            var validElement = new ElementBuilder()
                .WithQuestionId("question")
                .WithType(EElementType.Textarea)
                .Build();

            var element2 = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .WithTargetMapping(targetMapping)
                .Build();

            var page = new PageBuilder()
                .WithElement(validElement)
                .WithElement(element2)
                .Build();

            pages.Add(page);
            page = new PageBuilder()
                .WithElement(validElement)
                .Build();

            pages.Add(page);

            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckForInvalidQuestionOrTargetMappingValue(pages, "formName"));
            Assert.StartsWith("The provided json 'formName' contains invalid QuestionIDs or TargetMapping, ",
                result.Message);
        }

        [Theory]
        [InlineData("validquestionId", "")]
        [InlineData("valid.question", "")]
        [InlineData("valid.question.id", "")]
        [InlineData("", "validtagretMapping")]
        [InlineData("", "valid.target")]
        [InlineData("", "valid.target.mapping")]
        public void
        CheckForInvalidQuestionOrTargetMappingValue_ShouldNotThrowExceptionWhen_ValidQuestionId_OrTargetMapping(
                string questionId, string targetMapping)
        {
            var pages = new List<Page>();

            var validElement = new ElementBuilder()
                .WithQuestionId("question")
                .WithType(EElementType.Textarea)
                .Build();

            var element2 = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Textarea)
                .WithTargetMapping(targetMapping)
                .Build();

            var page = new PageBuilder()
                .WithElement(validElement)
                .WithElement(element2)
                .Build();

            pages.Add(page);
            page = new PageBuilder()
                .WithElement(validElement)
                .Build();

            pages.Add(page);

            _pageHelper.CheckForInvalidQuestionOrTargetMappingValue(pages, "formName");
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_ShouldThrowException_WhenNoConfigFound_ForForm()
        {
            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            var result = await Assert.ThrowsAsync<ApplicationException>(() =>
                _pageHelper.CheckForPaymentConfiguration(pages, "no-form-config"));
            Assert.Equal("No payment information configured for no-form-config form", result.Message);
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_ShouldNot_ThrowException_WhenConfigFound_ForForm_WithProvider()
        {
            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation> { new PaymentInformation { FormName = "test-form", PaymentProvider = "testProvider", Settings = new Settings() } });

            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            await _pageHelper.CheckForPaymentConfiguration(pages, "test-form");
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_Should_VerifyCalculationSlugs_StartWithHttps()
        {
            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation> { new PaymentInformation { FormName = "test-form", PaymentProvider = "testProvider", Settings = new Settings { ComplexCalculationRequired = true } } });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var element = new ElementBuilder()
                .WithType(EElementType.PaymentSummary)
                .WithCalculationSlugs(new SubmitSlug { Environment = "non-local", URL = "https://www.test.com" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithElement(element)
                .Build();

            pages.Add(page);

            await _pageHelper.CheckForPaymentConfiguration(pages, "test-form");
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_Should_ThrowException_WhenCalculateCostUrl_DoesNot_StartWithHttps()
        {
            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation> { new PaymentInformation { FormName = "test-form", PaymentProvider = "testProvider", Settings = new Settings { ComplexCalculationRequired = true } } });

            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("non-local");

            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var element = new ElementBuilder()
                .WithType(EElementType.PaymentSummary)
                .WithCalculationSlugs(new SubmitSlug { Environment = "non-local", URL = "http://www.test.com" })
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithElement(element)
                .Build();

            pages.Add(page);

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _pageHelper.CheckForPaymentConfiguration(pages, "test-form"));
            Assert.Equal("PaymentSummary::CalculateCostUrl must start with https", result.Message);
        }

        [Fact]
        public async Task
        CheckForPaymentConfiguration_ShouldThrowException_WhenPaymentProvider_DoesNotExists_WhenConfig_IsFound()
        {
            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            var result = await Assert.ThrowsAsync<ApplicationException>(() =>
                _pageHelper.CheckForPaymentConfiguration(pages, "test-form-with-invorrect-provider"));
            Assert.Equal("No payment provider configured for provider invalidProvider", result.Message);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldThrowAnException_WhenSubmitSlugAndPageSlugAreEmpty()
        {
            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            var result =
                Assert.Throws<ApplicationException>(() => _pageHelper.CheckForEmptyBehaviourSlugs(pages, "end-point"));
            Assert.Equal($"Incorrectly configured behaviour slug was discovered in end-point form", result.Message);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldNotThrowAnException_WhenSubmitSlugIsNotEmpty()
        {
            var pages = new List<Page>();

            var submitSlug = new SubmitSlug
            {
                Environment = "local",
                URL = "test-url"
            };

            var behaviour = new BehaviourBuilder()
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            _pageHelper.CheckForEmptyBehaviourSlugs(pages, "end-point");

            Assert.Single(pages[0].Behaviours[0].SubmitSlugs);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldNotThrowAnException_WhenPageSlugIsNotEmpty()
        {
            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithPageSlug("page-slug")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            _pageHelper.CheckForEmptyBehaviourSlugs(pages, "end-point");

            Assert.Equal("page-slug", pages[0].Behaviours[0].PageSlug);
        }

        [Fact]
        public void CheckForCurrentEnvironmentSubmitSlugs_ShouldThrowAnException_WhenPageSlugIsNotPresentFor()
        {
            var pages = new List<Page>();

            var submitSlug = new SubmitSlug
            {
                Environment = "mysteryEnvironment",
                URL = "test-url"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlug)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckForCurrentEnvironmentSubmitSlugs(pages, "end-point"));
            Assert.Equal($"No SubmitSlug found for end-point form for local", result.Message);
        }

        [Fact]
        public void
        CheckForAcceptedFileUploadFileTypes_ShouldThrowAnException_WhenAcceptedFleTypes_HasInvalidExtensionName()
        {
            var pages = new List<Page>();
            var invalidElementQuestionId = "fileUpload2";

            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("fileUpload")
                .WithAcceptedMimeType(".png")
                .Build();

            var elementWithInvalidMimeType = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId(invalidElementQuestionId)
                .WithAcceptedMimeType("png")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(elementWithInvalidMimeType)
                .Build();

            pages.Add(page);

            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckForAcceptedFileUploadFileTypes(pages, "end-point"));
            Assert.Equal(
                $"PageHelper::CheckForAcceptedFileUploadFileTypes, Allowed file type in FileUpload element {invalidElementQuestionId} must have a valid extension which begins with a ., e.g. .png",
                result.Message);
        }

        [Fact]
        public void CheckForAcceptedFileUploadFileTypes_ShouldNotThrowException_WhenAllFileTypesAreValid()
        {
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".png")
                .WithAcceptedMimeType(".pdf")
                .WithAcceptedMimeType(".jpg")
                .WithQuestionId("fileUpload")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".png")
                .WithAcceptedMimeType(".jpg")
                .WithAcceptedMimeType(".jpge")
                .WithQuestionId("fileUpload1")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".docx")
                .WithQuestionId("fileUpload3")
                .Build();

            var element4 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithAcceptedMimeType(".docx")
                .WithAcceptedMimeType(".doc")
                .WithQuestionId("fileUpload4")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element3)
                .WithElement(element4)
                .Build();

            pages.Add(page);
            pages.Add(page2);

            _pageHelper.CheckForAcceptedFileUploadFileTypes(pages, "end-point");
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldThrowApplicationException_WhenFormSchemaContains_NoDocumentTypes_WhenDocumentDownload_True()
        {
            var pages = new List<Page>();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .Build();


            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForDocumentDownload(formSchema));

            Assert.Equal("PageHelper::CheckForDocumentDownload, No document download type configured", result.Message);
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldThrowApplicationException_WhenFormSchemaContains_UnknownDocumentType_WhenDocumentDownload_True()
        {
            var pages = new List<Page>();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Unknown)
                .Build();


            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForDocumentDownload(formSchema));

            Assert.Equal("PageHelper::CheckForDocumentDownload, Unknown document download type configured",
                result.Message);
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldThrowApplicationException_WhenFormSchemaContains_UnknownDocumentType_InList_WhenDocumentDownload_True()
        {
            var pages = new List<Page>();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Unknown)
                .Build();


            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForDocumentDownload(formSchema));

            Assert.Equal("PageHelper::CheckForDocumentDownload, Unknown document download type configured",
                result.Message);
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldNotThrowApplicationException_WhenValidFormSchema_ForDocumentDownload()
        {
            var pages = new List<Page>();

            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Txt)
                .Build();


            _pageHelper.CheckForDocumentDownload(formSchema);
        }

        [Fact]
        public void CheckForAcceptedFileUploadFileTypes_ShouldNotThrowException_WhenNoFileUploadElementsExists()
        {
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("textBox")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("textArea")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .Build();

            pages.Add(page);

            _pageHelper.CheckForAcceptedFileUploadFileTypes(pages, "end-point");
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldThrowException_WhenAuthTokenIsNullOrEmptyAndBehaviourTypeIsNotSubmitPowerAutomate()
        {
            var pages = new List<Page>();

            var submitSlugs = new SubmitSlug
            {
                URL = "test"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form"));

            Assert.Equal("No Auth Token found in the SubmitSlug for test-form form", result.Message);
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldThrowException_WhenUrlIsNull()
        {
            var pages = new List<Page>();

            var submitSlugs = new SubmitSlug
            {
                AuthToken = "this is auth token"
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form"));

            Assert.Equal("No URL found in the SubmitSlug for test-form form", result.Message);
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldThrowException_WhenUrlIsEmpty()
        {
            var pages = new List<Page>();

            var submitSlugs = new SubmitSlug
            {
                AuthToken = "this is auth token",
                URL = ""
            };

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form"));

            Assert.Equal("No URL found in the SubmitSlug for test-form form", result.Message);
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldNotThrowException_WhenAuthTokenAndUrlAreNotNullOrEmpty()
        {
            {
                var pages = new List<Page>();

                var submitSlugs = new SubmitSlug
                {
                    AuthToken = "this is auth token",
                    URL = "test"
                };

                var behaviour = new BehaviourBuilder()
                    .WithBehaviourType(EBehaviourType.SubmitForm)
                    .WithSubmitSlug(submitSlugs)
                    .Build();

                var page = new PageBuilder()
                    .WithBehaviour(behaviour)
                    .Build();

                pages.Add(page);

                _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form");
            }
        }

        [Fact]
        public void AddIncomingFormDataValues_ShouldThrowException_WhenIncomingValueIsNull()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithName("testName")
                .WithOptional(false)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var formData = new Dictionary<string, dynamic>();

            // Act & Assert
            var result = Assert.Throws<Exception>(() => _pageHelper.AddIncomingFormDataValues(page, formData));

            Assert.Equal("DictionaryExtensions::IncomingValue, FormData does not contains testName required value",
                result.Message);
        }

        [Fact]
        public void AddIncomingFormDataValues_ShouldReturnSingleObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var formData = new Dictionary<string, dynamic>
            {
                {"nameTest", "45.23645"}
            };

            // Act
            var result = _pageHelper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.False(result.ContainsKey("nameTest"));

        }

        [Fact]
        public void AddIncomingFormDataValues_ShouldReturnSingleObject_WithOptionalTrue()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var formData = new Dictionary<string, dynamic>();

            // Act
            var result = _pageHelper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void AddIncomingFormDataValues_ShouldReturnMultipleValuesObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .Build();

            var incomingValue2 = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest2")
                .WithName("nameTest2")
                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .WithIncomingValue(incomingValue2)
                .Build();

            var formData = new Dictionary<string, dynamic>
            {
                {"nameTest", "45.23645"},
                {"nameTest2", "-2.345"}
            };

            // Act
            var result = _pageHelper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.False(result.ContainsKey("nameTest"));
            Assert.True(result.ContainsKey("questionIdTest2"));
            Assert.True(result.ContainsValue("-2.345"));
            Assert.False(result.ContainsKey("nameTest2"));
        }

        [Fact]
        public void AddIncomingFormDataValues_ShouldCall_RecursiveCheckAndCreate_AndReturnCorrectObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .Build();

            var incomingValue2 = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest2.nameTest2")
                .WithName("nameTest2")
                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .WithIncomingValue(incomingValue2)
                .Build();

            var formData = new Dictionary<string, dynamic>
            {
                {"nameTest", "45.23645"},
                {"questionIdTest2.nameTest2", "-2.345"}
            };

            // Act
            var result = _pageHelper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.False(result.ContainsKey("nameTest"));
            Assert.True(result.ContainsKey("questionIdTest2.nameTest2"));
            Assert.True(result.ContainsValue("-2.345"));
            Assert.False(result.ContainsKey("nameTest2"));
        }

        [Fact]
        public void CheckForIncomingFormDataValues_ShouldThrowException_WhenQuestionId_OR_Name_Empty()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValueWithNoQuestionId = new IncomingValuesBuilder()
                .WithQuestionId("")
                .WithName("testName")
                .Build();

            var incomingValueWithNoName = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithName("")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValueWithNoQuestionId)
                .WithIncomingValue(incomingValueWithNoName)
                .Build();

            var pages = new List<Page>
            {
                page
            };

            // Act & Assert
            var result = Assert.Throws<Exception>(() => _pageHelper.CheckForIncomingFormDataValues(pages));
            Assert.Equal("PageHelper::CheckForIncomingFormDataValues, QuestionId or Name cannot be empty", result.Message);
        }

        [Theory]
        [InlineData("", "questionId", "local",
            "PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction action type does not contain a url")]
        [InlineData("www.url.com", "", "local",
            "PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction action type does not contain a TargetQuestionId")]
        [InlineData("www.url.com", "questionId", "test",
            "PageHelper:CheckRetrieveExternalDataAction, RetrieveExternalDataAction there is no PageActionSlug for local")]
        public void
        CheckRetrieveExternalDataAction_ShouldThrowException_WhenActionDoesNotContain_URL_or_TargetQuestionId(
                string url, string questionId, string env, string message)
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.RetrieveExternalData)
                .WithPageActionSlug(new PageActionSlug
                {
                    URL = url,
                    Environment = env
                })
                .WithTargetQuestionId(questionId)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithFormActions(action)
                .Build();

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForPageActions(formSchema));
            Assert.Equal(message, result.Message);

        }

        [Theory]
        [InlineData("", "subject", "from", "to", "PageHelper:: CheckEmailAction, Content doesn't have a value")]
        [InlineData("content", "", "from", "to", "PageHelper:: CheckEmailAction, Subject doesn't have a value")]
        [InlineData("content", "subject", "", "to", "PageHelper:: CheckEmailAction, From doesn't have a value")]
        [InlineData("content", "subject", "from", "", "PageHelper:: CheckEmailAction, To doesn't have a value")]
        public void CheckEmailAction_ShouldThrowException_WhenActionDoesNotContain_Content_or_Subject_or_To_or_From(string content, string subject, string from, string to, string message)
        {
            // Arrange
            var action = new ActionBuilder()
                .WithActionType(EActionType.UserEmail)
                .WithContent(content)
                .WithSubject(subject)
                .WithFrom(from)
                .WithTo(to)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithFormActions(action)
                .Build();

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForPageActions(formSchema));
            Assert.Equal(message, result.Message);
        }

        [Fact]
        public void CheckRenderConditionsValid_ShouldThrowException_If_TwoOrMorePagesWithTheSameSlug_HaveNoRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var pages = new List<Page>
            {
                page,
                page2
            };

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckRenderConditionsValid(pages));
            Assert.Equal($"PageHelper:CheckRenderConditionsValid, More than one {page.PageSlug} page has no render conditions", result.Message);
        }

        [Fact]
        public void CheckRenderConditionsValid_ShouldThrowException_If_TwoOrMorePagesWithTheSameSlug_HaveEmptyRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page.RenderConditions = new List<Condition>();
            page2.RenderConditions = new List<Condition>();

            var pages = new List<Page>
            {
                page,
                page2
            };

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckRenderConditionsValid(pages));
            Assert.Equal($"PageHelper:CheckRenderConditionsValid, More than one {page.PageSlug} page has no render conditions", result.Message);
        }

        [Fact]
        public void CheckRenderConditionsValid_ShouldThrowException_If_TwoOrMorePagesWithTheSameSlug_HaveEmptyRenderConditions_And_TheLastPageHasRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            var page3 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            page.RenderConditions = new List<Condition>();
            page2.RenderConditions = new List<Condition>();

            var pages = new List<Page>
            {
                page,
                page2,
                page3
            };

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckRenderConditionsValid(pages));
            Assert.Equal($"PageHelper:CheckRenderConditionsValid, More than one {page.PageSlug} page has no render conditions", result.Message);
        }

        [Fact]
        public void CheckRenderConditionsValid_ShouldNotThrowException_If_TwoPagesHaveTheSameSlug_And_TheFirstOneHasRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page2.RenderConditions = new List<Condition>();

            var pages = new List<Page>
            {
                page,
                page2
            };

            // Act & Assert
            _pageHelper.CheckRenderConditionsValid(pages);
        }

        [Fact]
        public void CheckRenderConditionsValid_ShouldNotThrowException_If_TwoPagesWithTheSameSlugHaveRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "no"
                })
                .Build();

            var pages = new List<Page>
            {
                page,
                page2
            };

            // Act & Assert
            _pageHelper.CheckRenderConditionsValid(pages);
        }

        [Fact]
        public void CheckRenderConditionsValid_ShouldNotThrowException_If_TwoOrMorePagesWithTheSameSlugHaveRenderConditions_AndTheLastPageHasNoRenderConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "no"
                })
                .Build();

            var page3 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page3.RenderConditions = new List<Condition>();

            var pages = new List<Page>
            {
                page,
                page2,
                page3
            };

            // Act & Assert
            _pageHelper.CheckRenderConditionsValid(pages);
        }

        [Fact]
        public void GetPageWithMatchingRenderConditions_ShouldReturnPageWithNoConditions_If_ItDoesNotMeetTheConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "test",
                    EqualTo = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page2.RenderConditions = new List<Condition>();

            var pages = new List<Page>
            {
                page,
                page2
            };

            // Act
            var result = _pageHelper.GetPageWithMatchingRenderConditions(pages);

            // Assert
            Assert.Equal(page2, result);
            Assert.NotNull(result);
        }

        [Fact]
        public void GetPageWithMatchingRenderConditions_ShouldReturnPageWithRenderConditions_If_ItMeetsTheConditions()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testRadio",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page2.RenderConditions = new List<Condition>();

            var pages = new List<Page>
            {
                page,
                page2
            };

            var mockData = JsonConvert.SerializeObject(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                Response = "yes",
                                QuestionId = "testRadio"
                            }
                        }
                    }
                }
            });

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("guid");
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(mockData);

            // Act
            var result = _pageHelper.GetPageWithMatchingRenderConditions(pages);

            // Assert
            Assert.Equal(page, result);
            Assert.NotNull(result);
        }

        [Fact]
        public void GetPageWithMatchingRenderConditions_ShouldReturnPageWithNoConditions_IfFormDataIsNull()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .WithRenderConditions(new Condition
                {
                    QuestionId = "testRadio",
                    ConditionType = ECondition.EqualTo,
                    ComparisonValue = "yes"
                })
                .Build();

            var page2 = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithPageSlug("success")
                .Build();

            page2.RenderConditions = new List<Condition>();

            var pages = new List<Page>
            {
                page,
                page2
            };

            var mockData = JsonConvert.SerializeObject(new FormAnswers());

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("guid");
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(It.IsAny<string>());

            // Act
            var result = _pageHelper.GetPageWithMatchingRenderConditions(pages);

            // Assert
            Assert.Equal(page2, result);
            Assert.NotNull(result);
        }
    }
}