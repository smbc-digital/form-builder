using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.StorageProvider;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Booking.Request;
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
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new Mock<IHttpContextAccessor>();

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
                        FormName = "test-form-with-incorrect-provider",
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
            _pageHelper = new PageHelper(_mockIViewRender.Object,
                _mockElementHelper.Object, _mockDistributedCache.Object,
                _mockDisallowedKeysOptions.Object, _mockHostingEnv.Object,
                _mockCache.Object, _mockDistributedCacheExpirationSettings.Object,
                _mockPaymentProvider.Object, _mockSessionHelper.Object, _httpContextAccessor.Object);
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

            var pages = new List<Page> { page1, page2 };

            // Act & Assert
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

            var pages = new List<Page> { page1, page2 };

            // Act & Assert
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
            // Arrange
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

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForInvalidQuestionOrTargetMappingValue(pages, "formName"));

            // Assert
            Assert.StartsWith("The provided json 'formName' contains invalid QuestionIDs or TargetMapping, ", result.Message);
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
            // Arrange
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

            // Act & Assert
            _pageHelper.CheckForInvalidQuestionOrTargetMappingValue(pages, "formName");
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_ShouldThrowException_WhenNoConfigFound_ForForm()
        {
            // Arrange
            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _pageHelper.CheckForPaymentConfiguration(pages, "no-form-config"));

            // Assert
            Assert.Equal("No payment information configured for no-form-config form", result.Message);
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_ShouldNot_ThrowException_WhenConfigFound_ForForm_WithProvider()
        {
            // Arrange
            _mockCache
                .Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<List<PaymentInformation>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(new List<PaymentInformation>
                {
                    new PaymentInformation
                    {
                        FormName = "test-form",
                        PaymentProvider = "testProvider",
                        Settings = new Settings()
                    }
                });

            var pages = new List<Page>();
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();
            pages.Add(page);

            // Act
            await _pageHelper.CheckForPaymentConfiguration(pages, "test-form");

            // Assert
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_Should_VerifyCalculationSlugs_StartWithHttps()
        {
            // Arrange
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

            // Act
            await _pageHelper.CheckForPaymentConfiguration(pages, "test-form");
        }

        [Fact]
        public async Task CheckForPaymentConfiguration_Should_ThrowException_WhenCalculateCostUrl_DoesNot_StartWithHttps()
        {
            // Arrange
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

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _pageHelper.CheckForPaymentConfiguration(pages, "test-form"));
            Assert.Equal("PaymentSummary::CalculateCostUrl must start with https", result.Message);
        }

        [Fact]
        public async Task
        CheckForPaymentConfiguration_ShouldThrowException_WhenPaymentProvider_DoesNotExists_WhenConfig_IsFound()
        {
            // Arrange
            var pages = new List<Page>();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            // Act & Assert
            var result = await Assert.ThrowsAsync<ApplicationException>(() =>
                _pageHelper.CheckForPaymentConfiguration(pages, "test-form-with-incorrect-provider"));
            Assert.Equal("No payment provider configured for provider invalidProvider", result.Message);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldThrowAnException_WhenSubmitSlugAndPageSlugAreEmpty()
        {
            // Arrange
            var pages = new List<Page>();
            var behaviour = new BehaviourBuilder()
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            // Act & Assert
            var result =
                Assert.Throws<ApplicationException>(() => _pageHelper.CheckForEmptyBehaviourSlugs(pages, "end-point"));
            Assert.Equal($"Incorrectly configured behaviour slug was discovered in end-point form", result.Message);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldNotThrowAnException_WhenSubmitSlugIsNotEmpty()
        {
            // Arrange
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

            // Act
            _pageHelper.CheckForEmptyBehaviourSlugs(pages, "end-point");

            // Assert
            Assert.Single(pages[0].Behaviours[0].SubmitSlugs);
        }

        [Fact]
        public void CheckForEmptyBehaviourSlugs_ShouldNotThrowAnException_WhenPageSlugIsNotEmpty()
        {
            // Arrange
            var pages = new List<Page>();
            var behaviour = new BehaviourBuilder()
                .WithPageSlug("page-slug")
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            pages.Add(page);

            // Act
            _pageHelper.CheckForEmptyBehaviourSlugs(pages, "end-point");

            // Assert
            Assert.Equal("page-slug", pages[0].Behaviours[0].PageSlug);
        }

        [Fact]
        public void CheckForCurrentEnvironmentSubmitSlugs_ShouldThrowAnException_WhenPageSlugIsNotPresentFor()
        {
            // Arrange
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

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckForCurrentEnvironmentSubmitSlugs(pages, "end-point"));
            Assert.Equal($"No SubmitSlug found for end-point form for local", result.Message);
        }

        [Fact]
        public void
        CheckForAcceptedFileUploadFileTypes_ShouldThrowAnException_WhenAcceptedFleTypes_HasInvalidExtensionName()
        {
            // Arrange
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

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckForAcceptedFileUploadFileTypes(pages, "end-point"));
            Assert.Equal(
                $"PageHelper::CheckForAcceptedFileUploadFileTypes, Allowed file type in FileUpload element {invalidElementQuestionId} must have a valid extension which begins with a ., e.g. .png",
                result.Message);
        }

        [Fact]
        public void CheckForAcceptedFileUploadFileTypes_ShouldNotThrowException_WhenAllFileTypesAreValid()
        {
            // Arrange
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

            // Act
            _pageHelper.CheckForAcceptedFileUploadFileTypes(pages, "end-point");
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldNotThrowApplicationException_WhenConditionalElementIsFoundInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .Build();

            var pages = new List<Page> { page1 };

            // Act & Assert
            _pageHelper.CheckConditionalElementsAreValid(pages, "form");
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldNotThrowApplicationException_WhenConditionalElementIdIsBlankInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .Build();

            var pages = new List<Page> { page1 };

            // Act & Assert
            _pageHelper.CheckConditionalElementsAreValid(pages, "form");
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldThrowApplicationException_WhenConditionalElementNotFoundInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .Build();

            var pages = new List<Page> { page1 };

            // Act & Assert
            Assert.Throws<ApplicationException>(() => _pageHelper.CheckConditionalElementsAreValid(pages, "form"));
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldThrowApplicationException_WhenTooManyConditionalElementsFoundInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion2")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .WithElement(element2)
                .WithElement(element3)
                .Build();

            var pages = new List<Page> { page1 };

            // Act & Assert
            Assert.Throws<ApplicationException>(() => _pageHelper.CheckConditionalElementsAreValid(pages, "form"));
        }

        [Fact]
        public void CheckConditionalElementsAreValid_ShouldThrowApplicationException_WhenConditionalElementIsPlacedOnAnotherPageInJson()
        {
            // Arrange
            var option1 = new Option { ConditionalElementId = "conditionalQuestion1", Value = "Value1" };
            var element1 = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("radio")
                .WithLabel("First name")
                .WithOptions(new List<Option> { option1 })
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("conditionalQuestion1")
                .WithLabel("First name")
                .WithConditionalElement(true)
                .Build();

            var page1 = new PageBuilder()
                .WithElement(element1)
                .Build();

            var page2 = new PageBuilder()
                .WithElement(element2)
                .Build();

            var pages = new List<Page> { page1, page2 };

            // Act & Assert
            Assert.Throws<ApplicationException>(() => _pageHelper.CheckConditionalElementsAreValid(pages, "form"));
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldThrowApplicationException_WhenFormSchemaContains_NoDocumentTypes_WhenDocumentDownload_True()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .Build();

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForDocumentDownload(formSchema));

            // Assert
            Assert.Equal("PageHelper::CheckForDocumentDownload, No document download type configured", result.Message);
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldThrowApplicationException_WhenFormSchemaContains_UnknownDocumentType_WhenDocumentDownload_True()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Unknown)
                .Build();

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForDocumentDownload(formSchema));

            // Assert
            Assert.Equal("PageHelper::CheckForDocumentDownload, Unknown document download type configured",
                result.Message);
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldThrowApplicationException_WhenFormSchemaContains_UnknownDocumentType_InList_WhenDocumentDownload_True()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Unknown)
                .Build();

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForDocumentDownload(formSchema));

            // Assert
            Assert.Equal("PageHelper::CheckForDocumentDownload, Unknown document download type configured",
                result.Message);
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldNotThrowApplicationException_WhenValidFormSchema_ForDocumentDownload()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Txt)
                .Build();

            // Act
            _pageHelper.CheckForDocumentDownload(formSchema);
        }

        [Fact]
        public void CheckForAcceptedFileUploadFileTypes_ShouldNotThrowException_WhenNoFileUploadElementsExists()
        {
            // Arrange
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

            // Act
            _pageHelper.CheckForAcceptedFileUploadFileTypes(pages, "end-point");
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldThrowException_WhenAuthTokenIsNullOrEmptyAndBehaviourTypeIsNotSubmitPowerAutomate()
        {
            // Arrange
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

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form"));
            Assert.Equal("No Auth Token found in the SubmitSlug for test-form form", result.Message);
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldThrowException_WhenUrlIsNull()
        {
            // Arrange
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

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form"));
            Assert.Equal("No URL found in the SubmitSlug for test-form form", result.Message);
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldThrowException_WhenUrlIsEmpty()
        {
            // Arrange
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

            // Act & Assert
            var result = Assert.Throws<ApplicationException>(() =>
                _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form"));
            Assert.Equal("No URL found in the SubmitSlug for test-form form", result.Message);
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_ShouldNotThrowException_WhenAuthTokenAndUrlAreNotNullOrEmpty()
        {
            // Arrange
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

            // Act
            _pageHelper.CheckSubmitSlugsHaveAllProperties(pages, "test-form");
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
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var incomingValueWithNoName = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithName("")
                .WithHttpActionType(EHttpActionType.Post)
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


        [Fact]
        public void CheckForIncomingFormDataValues_ShouldThrowException_WhenActionType_IsUnknown()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValueWithNoType = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithHttpActionType(EHttpActionType.Unknown)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValueWithNoType)
                .Build();

            var pages = new List<Page>
            {
                page
            };

            // Act & Assert
            var result = Assert.Throws<Exception>(() => _pageHelper.CheckForIncomingFormDataValues(pages));
            Assert.Equal("PageHelper::CheckForIncomingFormDataValues, EHttpActionType cannot be unknwon, set to Get or Post", result.Message);
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

            // Act
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

            // Act
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

            // Act
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

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("guid");
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(It.IsAny<string>());

            // Act
            var result = _pageHelper.GetPageWithMatchingRenderConditions(pages);

            // Assert
            Assert.Equal(page2, result);
            Assert.NotNull(result);
        }

        [Fact]
        public void CheckAddressNoManualTextIsSet_ShouldAllowSchema_IfDetailsTextIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithDisableManualAddress(true)
                .WithNoManualAddressDetailText("Test")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var pages = new List<Page> { page };

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("guid");
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(It.IsAny<string>());

            // Act
            _pageHelper.CheckAddressNoManualTextIsSet(pages);
        }

        [Fact]
        public void CheckAddressNoManualTextIsSet_ShouldThrowException_IfNoDetailsTextIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithDisableManualAddress(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var pages = new List<Page> { page };

            _mockSessionHelper.Setup(_ => _.GetSessionGuid()).Returns("guid");
            _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(It.IsAny<string>());

            // Act & Assert
            Assert.Throws<ApplicationException>(() => _pageHelper.CheckAddressNoManualTextIsSet(pages));
        }

        [Fact]
        public void CheckForAnyConditionType_ShouldThrowException_IfComparisonValueIsNullOrEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            var condition = new ConditionBuilder()
                .WithConditionType(ECondition.Any)
                .WithQuestionId("test")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithCondition(condition)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var pages = new List<Page> { page };

            // Act & Assert
            Assert.Throws<ApplicationException>(() => _pageHelper.CheckForAnyConditionType(pages));
        }

        [Fact]
        public void CheckForAnyConditionType_ShouldAllowSchema_IfComparisonValueIsSet()
        {
            // Arrange
            var element = new ElementBuilder()
          .WithType(EElementType.Textbox)
          .Build();

            var condition = new ConditionBuilder()
                .WithConditionType(ECondition.Any)
                .WithComparisonValue("compValue")
                .WithQuestionId("test")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithCondition(condition)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var pages = new List<Page> { page };

            // Act
            _pageHelper.CheckForAnyConditionType(pages);
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
            var itemData = Assert.IsType<List<FileUploadModel>>(results[2].Response);
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
        public void SaveFormFileAnswer_ShouldNotSave_ExisitingFiles_IfUploadedTwice_InDistributedCache()
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
        public void CheckUploadedFilesSummaryQuestionsIsSet_ShouldThrowException_WhenElementDoNotContain_RequiredFileUpload_QuestionIds()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithPropertyText("label text")
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string>())
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckUploadedFilesSummaryQuestionsIsSet(pages));

            // Assert
            Assert.Equal("PageHelper:CheckUploadedFilesSummaryQuestionsIsSet, Uploaded files summary must have atleast one file questionId specified to display the list of uploaded files.", result.Message);
        }

        [Fact]
        public void CheckUploadedFilesSummaryQuestionsIsSet_ShouldThrowException_WhenElementDoNotContain_Required_Text()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.UploadedFilesSummary)
                .WithPropertyText(string.Empty)
                .WithFileUploadQuestionIds(new List<string> { "" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckUploadedFilesSummaryQuestionsIsSet(pages));

            // Assert
            Assert.Equal("PageHelper:CheckUploadedFilesSummaryQuestionsIsSet, Uploaded files summary text must not be empty.", result.Message);
        }

        [Fact]
        public void CheckUploadedFilesSummaryQuestionsIsSet_ShouldNot_ThrowException_WhenElement_Has_RequiredFileUpload_QuestionIds()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithQuestionId("question")
                .WithPropertyText("label text")
                .WithType(EElementType.UploadedFilesSummary)
                .WithFileUploadQuestionIds(new List<string> { "question-one" })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            _pageHelper.CheckUploadedFilesSummaryQuestionsIsSet(pages);
        }

        [Fact]
        public void CheckForBookingElement_Throw_ApplicationException_WhenForm_DoenotContain_RequiredCustomerFields()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("Fake")
                .WithAppointmentType(Guid.NewGuid())
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var appointment = new PageBuilder()
                .WithPageSlug(BookingConstants.NO_APPOINTMENT_AVAILABLE)
                .Build();

            pages.Add(page);
            pages.Add(appointment);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForBookingElement(pages));
            Assert.Equal("PageHelper:CheckForBookingElement, Booking element requires customer firstname/lastname elements for reservation", result.Message);
        }

        [Fact]
        public void CheckForBookingElement_Throw_ApplicationException_WhenForm_DoenotContain_Required_NoAppointmentsPage()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("Fake")
                .WithAppointmentType(Guid.NewGuid())
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForBookingElement(pages));
            Assert.Equal($"PageHelper:CheckForBookingElement, Form contains booking element but is missing required page with slug {BookingConstants.NO_APPOINTMENT_AVAILABLE}.", result.Message);
        }

        [Fact]
        public void CheckForBookingElement_Throw_ApplicationException_WhenBookingElement_DoesNotContains_AppointmentType()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("Fake")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForBookingElement(pages));
            Assert.Equal("PageHelper:CheckForBookingElement, Booking element requires a AppointmentType property.", result.Message);
        }

        [Fact]
        public void CheckForBookingElement_Throw_ApplicationException_WhenBookingElement_DoesNotContains_BookingProvider()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithAppointmentType(Guid.NewGuid())
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForBookingElement(pages));
            Assert.Equal("PageHelper:CheckForBookingElement, Booking element requires a valid booking provider property.", result.Message);
        }

        [Fact]
        public void CheckForBookingElement_Throw_ApplicationException_WhenBookingElement_Contains_EmptyGuid_ForOptionalResources()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithBookingProvider("TestProvider")
                .WithAppointmentType(Guid.NewGuid())
                .WithBookingResource(new BookingResource { ResourceId = Guid.Empty, Quantity = 1 })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForBookingElement(pages));
            Assert.Equal("PageHelper:CheckForBookingElement, Booking element optional resources are invalid, ResourceId cannot be an empty Guid.", result.Message);
        }

        [Fact]
        public void CheckForBookingElement_Throw_ApplicationException_WhenBookingElement_Contains_ZeroQuantity_ForOptionalResources()
        {
            // Arrange
            var pages = new List<Page>();

            var element = new ElementBuilder()
                .WithType(EElementType.Booking)
                .WithQuestionId("booking")
                .WithAppointmentType(Guid.NewGuid())
                .WithBookingProvider("TestProvider")
                .WithBookingResource(new BookingResource { ResourceId = Guid.NewGuid(), Quantity = 0 })
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            pages.Add(page);

            // Act
            var result = Assert.Throws<ApplicationException>(() => _pageHelper.CheckForBookingElement(pages));
            Assert.Equal("PageHelper:CheckForBookingElement, Booking element optional resources are invalid, cannot have a quantity less than 0", result.Message);
        }
    }
}