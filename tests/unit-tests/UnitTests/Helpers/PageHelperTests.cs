﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.FileStorage;
using form_builder.Providers.Lookup;
using form_builder.Providers.StorageProvider;
using form_builder.Services.RetrieveExternalDataService.Entities;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class PageHelperTests
    {
        private readonly PageHelper _pageHelper;
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new();
        private readonly Mock<IFileStorageProvider> _mockFileStorageProvider = new();
        private readonly Mock<IOptions<FormConfiguration>> _mockDisallowedKeysOptions = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistributedCacheExpirationSettings = new();
        private readonly Mock<ISessionHelper> _mockSessionHelper = new();
        private readonly List<ILookupProvider> _mockLookupProviders = new ();
        private readonly FakeLookupProvider _lookupProvider = new();
        private readonly Mock<IActionHelper> _mockActionHelper = new();

        public PageHelperTests()
        {
            _mockDisallowedKeysOptions.Setup(_ => _.Value).Returns(new FormConfiguration
            {
                DisallowedAnswerKeys = new[]
                {
                    "Guid", "Path"
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

            _mockLookupProviders.Add(_lookupProvider);

            _pageHelper = new PageHelper(_mockIViewRender.Object,
                _mockElementHelper.Object, _mockDistributedCache.Object,
                _mockDisallowedKeysOptions.Object, _mockHostingEnv.Object,
                _mockDistributedCacheExpirationSettings.Object,
                _mockSessionHelper.Object, _mockLookupProviders,
                _mockActionHelper.Object, _mockFileStorageProvider.Object);
        }

        [Fact]
        public async Task GenerateHtml_ShouldAddOptions_WhenFormContainsDynamicLookup()
        {
            //Arrange
            var element = new ElementBuilder().WithType(EElementType.Radio).WithLookup("dynamic").Build();
            element.Properties.LookupSources = new List<LookupSource>
            {
                new LookupSource
                {
                    EnvironmentName = "local",
                    Provider = "Fake",
                    AuthToken = "fake",
                    URL = "https://myapi.com"
                }
            };

            var page = new PageBuilder().WithElement(element).Build();

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(LookUpConstants.SubPathViewModelKey, LookUpConstants.Automatic);

            var schema = new FormSchemaBuilder().WithName("form-name").Build();
            var formAnswers = new FormAnswers();

            _mockActionHelper.Setup(_ => _.GenerateUrl("https://myapi.com", formAnswers)).Returns(new RequestEntity() { IsPost = false, Url = "waste" });

            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers, new List<object>());

            //Assert
            element = (Element)page.Elements.Single(x => !string.IsNullOrEmpty(x.Lookup) && x.Lookup.Equals("dynamic"));
            Assert.True(element.Properties.Options.Any());
        }

        [Fact]
        public async Task GenerateHtml_ShouldRenderH1Element_WithBaseFormName()
        {
            // Arrange
            var page = new PageBuilder()
                .WithPageTitle("Page title")
                .Build();
            var viewModel = new Dictionary<string, dynamic>();
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            // Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

            // Assert
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

            var formAnswers = new FormAnswers();
            //Act
            var result = await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == type.ToString()), It.IsAny<Element>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSelect()
        {
            //Arrange
            _mockElementHelper
                .Setup(_ => _.CurrentValue(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns("SK1 3XE");

            var element = (Address)new ElementBuilder()
                .WithType(EElementType.Address)
                .WithPropertyText("text")
                .WithQuestionId("address-test")
                .WithValue("SK1 3XE")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    LookUpConstants.SubPathViewModelKey, 
                    LookUpConstants.Automatic
                }
            };

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();
            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers, new List<object>());

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == "AddressSelect"),
                    It.IsAny<form_builder.Models.Elements.Address>(), null), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldGenerateValidUrl_ForAddressSelect()
        {
            //Arrange
            var callback = new Address();

            _mockIViewRender
                .Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Address>(), null))
                .Callback<string, Address, Dictionary<string, object>>((x, y, z) => callback = y);

            _mockElementHelper.Setup(_ => _.CurrentValue(It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormAnswers>(), It.IsAny<string>()))
                .Returns("SK1 3XE");

            var pageSlug = "page-one";
            var baseUrl = "test";

            var addressElement = new Address
            {
                Properties = new BaseProperty
                {
                    Text = "text"
                }
            };

            var page = new PageBuilder()
                .WithElement(addressElement)
                .WithPageSlug(pageSlug)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    LookUpConstants.SubPathViewModelKey,
                    LookUpConstants.Automatic
                }
            };

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .WithBaseUrl(baseUrl)
                .Build();

            var formAnswers = new FormAnswers();
            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers, new List<object>());

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}", callback.ReturnURL);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenAddressSearch()
        {
            //Arrange
            var addressElement = new Address
            {
                Properties = new BaseProperty
                {
                    Text = "text"
                }
            };

            var page = new PageBuilder()
                .WithElement(addressElement)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "subPath",
                    string.Empty
                }
            };

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("AddressSearch")),
                    It.IsAny<Address>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartial_WhenStreetSelect()
        {
            //Arrange
            var element = new Street
            { Properties = new BaseProperty { QuestionId = "street", StreetProvider = "test", Text = "test" } };

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    LookUpConstants.SubPathViewModelKey,
                    LookUpConstants.Automatic
                },
                {
                    "street-streetaddress",
                    string.Empty
                },
                {
                    "street-street",
                    "street"
                }
            };

            var schema = new FormSchemaBuilder()
                .WithName("Street name")
                .Build();

            var formAnswers = new FormAnswers();
            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("StreetSelect")),
                    It.IsAny<Street>(), null), Times.Once);
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

            var formAnswers = new FormAnswers();

            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x.Equals("StreetSearch")), It.IsAny<Element>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task GenerateHtml_ShouldGenerateValidUrl_ForStreetSelect()
        {
            //Arrange
            var callback = new Street();
            _mockIViewRender
                .Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Street>(), null))
                .Callback<string, Street, Dictionary<string, object>>((x, y, z) => callback = y);

            var pageSlug = "page-one";
            var baseUrl = "test";
            var element = new Street
            {
                Properties = new BaseProperty
                {
                    Text = "test"
                }
            };

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

            var formAnswers = new FormAnswers();
            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

            //Assert
            Assert.Equal($"/{baseUrl}/{pageSlug}", callback.ReturnURL);
        }

        [Theory]
        [InlineData(EElementType.OL)]
        [InlineData(EElementType.UL)]
        public async Task GenerateHtml_ShouldCallViewRenderWithCorrectPartialForList(EElementType type)
        {
            //Arrange
            var listItems = new List<string> { "item 1", "item 2", "item 3" };

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

            var formAnswers = new FormAnswers();
            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

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

            var formAnswers = new FormAnswers();
            //Act
            await _pageHelper.GenerateHtml(page, viewModel, schema, string.Empty, formAnswers);

            //Assert
            _mockIViewRender.Verify(
                _ => _.RenderAsync(It.Is<string>(x => x == EElementType.Img.ToString()), It.IsAny<Element>(),
                    It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public void SaveAnswers_ShouldCallCacheProvider()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var viewModel = new Dictionary<string, dynamic> { { "Path", "path" } };
            var mockData = JsonConvert.SerializeObject(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
            });

            _mockDistributedCache
                .Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            // Act
            _pageHelper.SaveAnswers(viewModel, guid.ToString(), "formName", null, true);

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(x => x.Equals(guid.ToString()))));
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x.Equals(guid.ToString())),
                It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void SaveAnswers_ShouldRemoveCurrentPageData_IfPageKey_AlreadyExists()
        {
            // Arrange
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

            var viewModel = new Dictionary<string, dynamic>
            {
                {"Path", "path"},
                {"Item1", item1Data},
                {"Item2", item2Data}
            };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);
            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            // Assert
            Assert.Equal("Item1", callbackModel.Pages[0].Answers[0].QuestionId);
            Assert.Equal(item1Data, callbackModel.Pages[0].Answers[0].Response);
            Assert.Equal("Item2", callbackModel.Pages[0].Answers[1].QuestionId);
            Assert.Equal(item2Data, callbackModel.Pages[0].Answers[1].Response);
        }

        [Fact]
        public void SaveAnswers_ShouldNotAddKeys_OnDisallowedList()
        {
            // Arrange
            var callbackCacheProvider = string.Empty;

            _mockDistributedCache.Setup(_ =>
                    _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var mockData = JsonConvert.SerializeObject(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
            });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            var viewModel = new Dictionary<string, dynamic> { { "Path", "path" } };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);
            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            // Assert
            Assert.Empty(callbackModel.Pages[0].Answers);
        }

        [Fact]
        public void SaveAnswers_AddAnswersInViewModel()
        {
            // Arrange
            var callbackCacheProvider = string.Empty;
            var item1Data = "item1-data";
            var item2Data = "item2-data";
            var mockData = JsonConvert.SerializeObject(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
            });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache
                .Setup(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackCacheProvider = y);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"Path", "path"},
                {"Item1", item1Data},
                {"Item2", item2Data}
            };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);
            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            // Assert
            Assert.Equal("Item1", callbackModel.Pages[0].Answers[0].QuestionId);
            Assert.Equal(item1Data, callbackModel.Pages[0].Answers[0].Response);
            Assert.Equal("Item2", callbackModel.Pages[0].Answers[1].QuestionId);
            Assert.Equal(item2Data, callbackModel.Pages[0].Answers[1].Response);
        }

        [Fact]
        public void SaveAnswers_ShouldSaveFileUpload_WithinDistributedCache_OnSeperateKey()
        {
            // Arrange
            var questionId = "fileUpload_testFileQuestionId";
            var fileContent = "abc";
            var fileName = "fileName.txt";
            var collection = new List<CustomFormFile>();
            var fileMock = new CustomFormFile(fileContent, questionId, 0, fileName);
            collection.Add(fileMock);

            var allTheAnswers = new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
            };
            var mockData = JsonConvert.SerializeObject(allTheAnswers);

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            var viewModel = new Dictionary<string, dynamic>
            {
                {"Path", "path"},
                {questionId, new DocumentModel {Content = fileContent}}
            };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", collection, true);

            // Assert
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<int>(_ => _ == 60), It.IsAny<CancellationToken>()), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void SaveAnswers_ShouldSaveFilUploadReference_WithinFormAnswers_InDistributedCache()
        {
            // Arrange
            var callbackCacheProvider = string.Empty;
            var questionId = "fileUpload_testFileQuestionId";
            var fileContent = "abc";
            var fileName = "fileName.txt";

            var collection = new List<CustomFormFile>();
            var fileMock = new CustomFormFile(fileContent, questionId, 0, fileName);
            collection.Add(fileMock);

            var allTheAnswers = new List<Answers>
            {
                new Answers
                {
                    QuestionId = questionId,
                    Response = new FileUploadModel
                    {
                        Key = questionId,
                        TrustedOriginalFileName = $"file-{questionId}",
                        UntrustedOriginalFileName = fileName
                    }
                }
            };

            var form = new FormAnswers
            {
                FormName = "testpage",
                Path = "page-one",
                Pages = new List<PageAnswers>()
            };

            var page = new PageAnswers
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

            var viewModel = new Dictionary<string, dynamic>
            {
                {"Path", "path"},
                {questionId, new DocumentModel {Content = fileContent}}
            };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", collection, true);
            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            // Assert
            Assert.Equal(questionId, callbackModel.Pages[0].Answers[0].QuestionId);
            var fileUploadModel = JsonConvert.DeserializeObject<FileUploadModel>(callbackModel.Pages[0].Answers[0].Response.ToString());
            Assert.Equal(questionId, fileUploadModel.Key);
        }

        [Fact]
        public void
        SaveAnswers_ShouldReplaceFilUploadReference_WithinFormAnswers_IfAnswerAlreadyExists_InDistributedCache()
        {
            // Arrange
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

            var form = new FormAnswers
            {
                FormName = "testpage",
                Path = "pageone",
                Pages = new List<PageAnswers>()
            };

            var page = new PageAnswers
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

            var viewModel = new Dictionary<string, dynamic>
            {
                {"Path", "page-one"},
                {questionId, new DocumentModel {Content = fileContent}}
            };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", collection, true);
            var callbackModel = JsonConvert.DeserializeObject<FormAnswers>(callbackCacheProvider);

            // Assert
            Assert.Equal(questionId, callbackModel.Pages[0].Answers[0].QuestionId);
            FileUploadModel fileUploadModel = JsonConvert.DeserializeObject<FileUploadModel>(callbackModel.Pages[0].Answers[0].Response.ToString());
            Assert.Equal(fileName, fileUploadModel.UntrustedOriginalFileName);
        }

        [Fact]
        public void SaveAnswers_ShouldNotCallDistributedCache_ForFileUpload_WhenNoFile()
        {
            // Arrange
            var mockData = JsonConvert.SerializeObject(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>()
            });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            var viewModel = new Dictionary<string, dynamic> { { "Path", "path" } };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true);

            // Arrange
            _mockDistributedCache.Verify(
                _ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()), Times.Never);
            _mockDistributedCache.Verify(
                _ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public void SaveAnswers_ShouldSave_CurrentPageAnswer_WhenNoFilesSelected_And_PageAlreadyHasData_ForMultipleFileUpload()
        {
            // Arrange
            var callbackValue = string.Empty;
            var mockData = JsonConvert.SerializeObject(new FormAnswers
            {
                Path = "page-one",
                Pages = new List<PageAnswers>
                {
                    new PageAnswers
                    {
                        PageSlug = "page-two",
                        Answers = new List<Answers>
                        {
                            new Answers
                            {
                                QuestionId = "fileupload",
                                Response = JsonConvert.SerializeObject(new List<FileUploadModel>{ new FileUploadModel() })
                            }
                        }
                    }
                }
            });

            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(mockData);

            _mockDistributedCache.Setup(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackValue = y);

            var viewModel = new Dictionary<string, dynamic> { { "Path", "page-two" } };

            // Act
            _pageHelper.SaveAnswers(viewModel, Guid.NewGuid().ToString(), "formName", null, true, true);

            // Arrange
            _mockDistributedCache.Verify(
                _ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
            var result = JsonConvert.DeserializeObject<FormAnswers>(callbackValue);
            Assert.Single(result.Pages);
            Assert.Single(result.Pages.FirstOrDefault().Answers);
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

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("guid");
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(It.IsAny<string>());

            // Act
            var result = _pageHelper.GetPageWithMatchingRenderConditions(pages);

            // Assert
            Assert.Equal(page2, result);
            Assert.NotNull(result);
        }

        [Fact]
        public void SaveFormFileAnswer_ShouldInsertFilesToAnswers_IfAnswerDontExist()
        {
            // Arrange                                        
            var questionId = "fileUpload_FileQuestionId";
            var file = new List<CustomFormFile>();
            file.Add(new CustomFormFile("abc", questionId, 0, "replace-me.txt"));

            var page = new PageAnswers
            {
                PageSlug = "path",
                Answers = new List<Answers>
                {
                    new Answers { QuestionId = "Item1", Response = JsonConvert.SerializeObject(new List<FileUploadModel>{ new FileUploadModel()}) },
                    new Answers { QuestionId = "Item2", Response = JsonConvert.SerializeObject(new List<FileUploadModel>{ new FileUploadModel()}) }
                }
            };

            // Act
            var results = _pageHelper.SaveFormFileAnswers(page.Answers, file, false, page);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(3, results.Count());
            Assert.IsType<List<FileUploadModel>>(results[2].Response);
            Assert.StartsWith("file-fileUpload_FileQuestionId-", results[2].Response[0].Key);
        }

        [Fact]
        public void SaveFormFileAnswer_ShouldUpdateResponseFileForMultipleUpload_IfAnswerExist()
        {
            // Arrange                                        
            var questionId = "Item1";
            var currentAnswerKey = $"file-{questionId}-{Guid.NewGuid()}";
            var file = new List<CustomFormFile>();
            file.Add(new CustomFormFile(null, questionId, 1, null));

            var fileUpload = new List<FileUploadModel>();
            fileUpload.Add(
                new FileUploadModel
                {
                    Key = currentAnswerKey,
                    TrustedOriginalFileName = WebUtility.HtmlEncode("replace-me.txt"),
                    UntrustedOriginalFileName = "replace-me.txt",
                    FileSize = 0
                }
            );

            var page = new PageAnswers
            {
                PageSlug = "path",
                Answers = new List<Answers>
                {
                    new Answers { QuestionId = "Item1", Response = JsonConvert.SerializeObject(fileUpload) }
                }
            };

            // Act
            var results = _pageHelper.SaveFormFileAnswers(page.Answers, file, true, page);

            // Assert
            Assert.NotNull(results);
            var itemData = Assert.IsType<List<FileUploadModel>>(results[0].Response);
            Assert.Equal(2, itemData.Count);
            Assert.Equal(currentAnswerKey, itemData[0].Key);
            Assert.StartsWith($"file-{questionId}-", itemData[1].Key);
        }

        [Fact]
        public void SaveFormFileAnswer_ShouldUpdateResponseFileForSingleUpload_IfAnswerExist()
        {
            // Arrange                                        
            var questionId = "Item1";
            var file = new List<CustomFormFile>();
            file.Add(new CustomFormFile(null, questionId, 1, null));

            var fileUpload = new List<FileUploadModel>();
            fileUpload.Add(
                new FileUploadModel
                {
                    Key = $"file-OLD-ANSWER",
                    TrustedOriginalFileName = WebUtility.HtmlEncode("replace-me.txt"),
                    UntrustedOriginalFileName = "replace-me.txt",
                    FileSize = 0
                }
            );

            var page = new PageAnswers
            {
                PageSlug = "path",
                Answers = new List<Answers>
                {
                    new Answers { QuestionId = "Item1", Response = JsonConvert.SerializeObject(fileUpload) }
                }
            };

            // Act
            var results = _pageHelper.SaveFormFileAnswers(page.Answers, file, false, page);

            // Assert
            Assert.NotNull(results);
            var itemData = Assert.IsType<List<FileUploadModel>>(results[0].Response);
            Assert.Single(itemData);
            Assert.StartsWith($"file-{questionId}-", itemData[0].Key);
        }


        [Fact]
        public void SaveFormFileAnswer_ShouldSave_Files_InDistributedCache()
        {
            // Arrange                                        
            var questionId = "fileUpload";
            var file = new List<CustomFormFile>();
            file.Add(new CustomFormFile("content", questionId, 1, "fileone.txt"));
            file.Add(new CustomFormFile("content", questionId, 1, "filetwo.txt"));
            var page = new PageAnswers
            {
                PageSlug = "path",
                Answers = new List<Answers>()
            };

            // Act
            _pageHelper.SaveFormFileAnswers(page.Answers, file, true, page);

            // Assert
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x.StartsWith($"file-{questionId}-")), It.IsAny<string>(), It.Is<int>(_ => _ == 60), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public void SaveFormFileAnswer_ShouldNotSave_ExistingFiles_IfUploadedTwice_InDistributedCache()
        {
            // Arrange
            var questionId = "fileUpload";
            var file = new List<CustomFormFile>();
            file.Add(new CustomFormFile("content", questionId, 1, "newfile.txt"));
            file.Add(new CustomFormFile("content", questionId, 1, "existingfile.txt"));
            file.Add(new CustomFormFile("content", questionId, 1, "existingfiletwo.txt"));

            var fileUpload = new List<FileUploadModel>();
            fileUpload.Add(
              new FileUploadModel
              {
                  Key = questionId,
                  TrustedOriginalFileName = WebUtility.HtmlEncode("existingfile.txt"),
                  UntrustedOriginalFileName = "existingfile.txt",
                  FileSize = 0
              }
            );
            fileUpload.Add(
            new FileUploadModel
            {
                Key = questionId,
                TrustedOriginalFileName = WebUtility.HtmlEncode("existingfiletwo.txt"),
                UntrustedOriginalFileName = "existingfiletwo.txt",
                FileSize = 0
            });

            var page = new PageAnswers
            {
                PageSlug = "path",
                Answers = new List<Answers>
                {
                    new Answers { QuestionId = questionId, Response = JsonConvert.SerializeObject(fileUpload) }
                }
            };

            // Act
            _pageHelper.SaveFormFileAnswers(page.Answers, file, true, page);

            // Assert
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x.StartsWith($"file-{questionId}-")), It.IsAny<string>(), It.Is<int>(_ => _ == 60), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void SaveNonQuestionAnswers_ShouldNotCallDistributedCache_WhenNoDataProvided()
        {
            // Arrange
            var guid = "12345";

            // Act
            _pageHelper.SaveNonQuestionAnswers(new Dictionary<string, object>(), "form", "path", guid);

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(x => x.Equals(guid))), Times.Never);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x.Equals(guid)), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void SaveNonQuestionAnswers_ShouldSet_NonQuestionAnswers_WhenProvidedData()
        {
            // Arrange
            var callbackValue = string.Empty;
            var guid = "12345";
            var data = new Dictionary<string, object> { { "test", "value" } };
            _mockDistributedCache.Setup(_ => _.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((x, y, z) => callbackValue = y);

            // Act
            _pageHelper.SaveNonQuestionAnswers(data, "form", "path", guid);

            // Assert
            _mockDistributedCache.Verify(_ => _.GetString(It.Is<string>(x => x.Equals(guid))), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(It.Is<string>(x => x.Equals(guid)), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            var callbackData = JsonConvert.DeserializeObject<FormAnswers>(callbackValue);
            Assert.Single(callbackData.AdditionalFormData);
        }

        [Fact]
        public void SavePaymentAmount_ShouldCallDistributedCache()
        {
            var sessionGuid = Guid.NewGuid().ToString();
            _pageHelper.SavePaymentAmount(sessionGuid, "10.00" );

            _mockDistributedCache.Verify(_ => _.GetString(sessionGuid), Times.Once);
            _mockDistributedCache.Verify(_ => _.SetStringAsync(sessionGuid, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}