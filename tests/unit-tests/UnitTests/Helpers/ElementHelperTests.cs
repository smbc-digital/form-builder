using System.Text;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class ElementHelperTests
    {
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new();
        private readonly Mock<IElementMapper> _mockElementMapper = new();
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment = new();
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
        private readonly ElementHelper _elementHelper;
        private readonly Mock<IElementHelper> _mockElementHelper = new();

        public ElementHelperTests()
        {
            _elementHelper = new ElementHelper(_mockDistributedCacheWrapper.Object,
                _mockElementMapper.Object,
                _mockWebHostEnvironment.Object,
                _mockHttpContextAccessor.Object);

            _mockWebHostEnvironment.Setup(_ => _.EnvironmentName).Returns("local");

            _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Host)
               .Returns(new HostString("www.test.com"));
        }

        [Fact]
        public void CurrentValue_ReturnsCurrentValueOfElement()
        {
            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add("test-id", "this is the value");

            // Act
            var result = _elementHelper.CurrentValue("test-id", viewModel, new FormAnswers());

            // Assert
            Assert.Equal("this is the value", result);
        }

        [Fact]
        public void CurrentValue_ReturnsNoValueOfElement()
        {
            var viewModel = new Dictionary<string, dynamic>
            {
                {"test-id2", "this is the value"}
            };

            // Act
            var result = _elementHelper.CurrentValue("test-id", viewModel, new FormAnswers());

            // Assert
            Assert.Equal(string.Empty, result);
        }


        [Fact]
        public void CurrentValue_ShouldReturnEmpty_WhenCacheDoesNotContainPageData()
        {
            // Arrange
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers>() }));

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _elementHelper.CurrentValue("test-id", viewModel, new FormAnswers(), string.Empty);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void CurrentValue_ShouldReturnStoredValueOfElement_WhenCacheDataContainsElementValue()
        {
            // Arrange
            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                    PageSlug = "test-slug",
                    Answers = new List<Answers>{
                        new Answers {
                            QuestionId = "test-id",
                            Response = "this is the value"
                            }
                        }
                    }
                }
            };

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _elementHelper.CurrentValue("test-id", viewModel, formAnswers);

            // Assert
            Assert.Equal("this is the value", result);
        }

        [Fact]
        public void CurrentValue_ShouldReturnEmpty_WhenCacheDataDoesNotContainElementValue()
        {
            // Arrange
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers
                {
                    Pages = new List<PageAnswers>
                    {
                        new PageAnswers {
                        PageSlug = "test-slug",
                        Answers = new List<Answers>()
                        }
                    }
                }));

            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var result = _elementHelper.CurrentValue("test-id", viewModel, new FormAnswers());

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForLabel_ReturnsTrue_IfLabelExists(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(elementType)
               .WithQuestionId("test-id")
               .WithLabel("test-text")
               .Build();

            // Act
            var result = _elementHelper.CheckForLabel(element);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForLabel_ThrowsException_IfLabelDoesNotExists(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
              .WithType(elementType)
              .WithQuestionId("test-id")
              .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(element));
        }

        [Theory]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForQuestionId_ReturnsTrue_IfQuestionIdExists(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(elementType)
                .WithQuestionId("test")
                .Build();

            // Act
            var result = _elementHelper.CheckForQuestionId(element);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForQuestionId_ThrowsException_IfQuestionIdDoesNotExist(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(elementType)
                .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForQuestionId(element));
        }

        [Fact]
        public void CheckAllDateRestrictionsAreNotEnabled_ReturnsTrue_IfNotAllRestrictionsAreTrue()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithRestrictCurrentDate(true)
                .Build();

            // Act
            var result = _elementHelper.CheckAllDateRestrictionsAreNotEnabled(element);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckAllDateRestrictionsAreNotEnabled_ThrowsException_IfAllRestrictionsAreTrue()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithRestrictCurrentDate(true)
                .WithRestrictFutureDate(true)
                .WithRestrictPastDate(true)
                .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckAllDateRestrictionsAreNotEnabled(element));
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void CheckForLabel_ThrowsException_IfLabelIsEmpty(EElementType elementType)
        {
            // Arrange
            var element = new ElementBuilder()
              .WithType(elementType)
              .WithQuestionId("test-id")
              .WithLabel(string.Empty)
              .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(element));
        }

        [Fact]
        public void CheckForMaxLength_ReturnsTrue_IfMaxLengthExists()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("issueOne")
               .WithMaxLength(2000)
               .Build();

            // Act
            var result = _elementHelper.CheckForMaxLength(element);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForMaxLength_ReturnsTrue_IfMaxLengthExistsAndAboveZero()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("issueOne")
               .WithMaxLength(1)
               .Build();

            // Act
            var result = _elementHelper.CheckForMaxLength(element);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForMaxLength_ThrowsException_IfMaxLengthExistsAndIsZero()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("issueOne")
               .WithMaxLength(0)
               .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForMaxLength(element));
        }

        [Fact]
        public void CheckIfLabelAndText_ReturnsTrue_IfLabelAndTextAreFilledIn()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.InlineAlert)
               .WithLabel("test-text")
               .WithPropertyText("Test")
               .Build();

            // Act
            var result = _elementHelper.CheckIfLabelAndTextEmpty(element);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckIfLabelAndTextEmpty_ThrowsException_IfLabelAndTextAreEmpty()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.InlineAlert)
               .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckIfLabelAndTextEmpty(element));
        }

        [Fact]
        public void CheckForRadioOptions_ShouldThrowException_IfNoOptionsAreGiven()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Radio)
               .WithQuestionId("questionId")
               .WithLabel("Label").Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForRadioOptions(element));
        }

        [Fact]
        public void CheckForRadioOptions_ShouldThrowException_IfOptionsAreEmpty()
        {
            //Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.Radio)
               .WithQuestionId("questionId")
               .WithLabel("Label")
               .WithOptions(new List<Option>())
               .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForRadioOptions(element));
        }

        [Fact]
        public void CheckForRadioOptions_ShouldThrowException_IfOneOption_And_AllowSingleOptionFalse()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("questionId")
                .WithLabel("Label")
                .WithOptions(new List<Option> { new Option() })
                .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForRadioOptions(element));
        }

        [Fact]
        public void CheckForRadioOptions_ShouldReturnTrue_IfOneOption_And_AllowSingleOptionTrue()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("questionId")
                .WithLabel("Label")
                .WithOptions(new List<Option> { new Option() })
                .WithAllowSingleOption(true)
                .Build();

            // Act
            var result = _elementHelper.CheckForRadioOptions(element);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckForRadioOptions_ShouldReturnTrue_IfIsInlineTrue()
        {
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("questionId")
                .WithLabel("Label")
                .WithOptions(new List<Option> { new Option(), new Option() })
                .WithIsInline(true)
                .Build();

            // Act
            var result = _elementHelper.CheckForRadioOptions(element);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(EElementType.P, "paragraph")]
        [InlineData(EElementType.H1, "Header 1")]
        [InlineData(EElementType.H2, "Header 2")]
        [InlineData(EElementType.H3, "Header 3")]
        [InlineData(EElementType.H4, "Header 4")]
        [InlineData(EElementType.H5, "Header 5")]
        [InlineData(EElementType.H6, "Header 6")]
        public void ElementBuilder_ShouldCreateGenericHtmlElementsWithText(EElementType eElementType, string text)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(eElementType)
                .WithPropertyText(text)
                .Build();

            // Act & Assert
            Assert.Equal(text, element.Properties.Text);
        }

        [Theory]
        [InlineData(EElementType.OL)]
        [InlineData(EElementType.UL)]
        public void ElementBuilder_ShouldCreateListsWithListItems(EElementType eElementType)
        {
            // Arrange
            var listItems = new List<string> { "item 1", "item 2", "item 3" };

            var element = new ElementBuilder()
                .WithType(eElementType)
                .WithListItems(listItems)
                .Build();

            // Act & Assert
            Assert.Equal(3, element.Properties.ListItems.Count);
            Assert.Equal("item 1", element.Properties.ListItems[0]);
        }

        [Fact]
        public void ElementBuilder_ShouldCreateImageElement_WithProperties()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Img)
                .WithAltText("alt text")
                .WithSource("source")
                .Build();

            // Act & Assert
            Assert.Equal("alt text", element.Properties.AltText);
            Assert.Equal("source", element.Properties.Source);
        }

        [Fact]
        public void ElementBuilder_ShouldCreateRadioSet_WithOptionsAndHintText()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("questionId")
                .WithLabel("Label")
                .WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1", Hint = "Option 1 Hint" },
                  new Option { Value = "option2", Text = "Option 2", Hint = "Option 2 Hint" } })
                .Build();

            // Act & Assert
            Assert.True(_elementHelper.CheckForRadioOptions(element));
            Assert.Equal("questionId", element.Properties.QuestionId);
            Assert.Equal("Label", element.Properties.Label);
            Assert.Equal("option1", element.Properties.Options[0].Value);
            Assert.Equal("Option 1", element.Properties.Options[0].Text);
            Assert.Equal("Option 1 Hint", element.Properties.Options[0].Hint);
            Assert.Equal("option2", element.Properties.Options[1].Value);
            Assert.Equal("Option 2", element.Properties.Options[1].Text);
            Assert.Equal("Option 2 Hint", element.Properties.Options[1].Hint);
        }

        [Fact]
        public void ElementBuilder_ShouldCreateSelect_WithTwoOptions()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId("questionId")
                .WithLabel("Label")
                .WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1"},
                  new Option { Value = "option2", Text = "Option 2"} })
                .Build();

            // Act & Assert
            Assert.True(_elementHelper.CheckForSelectOptions(element));
            Assert.Equal("questionId", element.Properties.QuestionId);
            Assert.Equal("Label", element.Properties.Label);
            Assert.Equal("option1", element.Properties.Options[0].Value);
            Assert.Equal("Option 1", element.Properties.Options[0].Text);
            Assert.Equal("option2", element.Properties.Options[1].Value);
            Assert.Equal("Option 2", element.Properties.Options[1].Text);
        }

        [Fact]
        public void ElementBuilder_ShouldCreateCheckBoxList()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithQuestionId("questionId")
                .WithLabel("Label").WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1"},
                  new Option { Value = "option2", Text = "Option 2"} })
                .Build();

            // Act & Assert
            Assert.Equal("questionId", element.Properties.QuestionId);
            Assert.Equal("Label", element.Properties.Label);
            Assert.Equal("option1", element.Properties.Options[0].Value);
            Assert.Equal("Option 1", element.Properties.Options[0].Text);
            Assert.Equal("option2", element.Properties.Options[1].Value);
            Assert.Equal("Option 2", element.Properties.Options[1].Text);
        }

        [Fact]
        public void ElementBuilder_ShouldThrowException_WithOneSelectOption()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId("questionId")
                .WithLabel("Label")
                .WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1"}})
                .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForSelectOptions(element));
        }

        [Fact]
        public void ElementBuilder_ShouldThrowException_WithNoLabel()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId("questionId")
                 .WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1"},
                  new Option { Value = "option2", Text = "Option 2"} })
                .Build();

            // Act & Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForLabel(element));
        }

        [Fact]
        public void CurrentValue_ReturnsCurrentDateValueOfElement()
        {
            // Arrange
            var questionId = "passportIssued";
            var dayId = questionId + "-day";
            var monthId = questionId + "-month";
            var yearId = questionId + "-year";

            var viewModel = new Dictionary<string, dynamic>();
            viewModel.Add(dayId, "14");
            viewModel.Add(monthId, "09");
            viewModel.Add(yearId, "2010");

            // Act
            var dayResult = _elementHelper.CurrentValue(questionId, viewModel, new FormAnswers(), "-day");
            var monthResult = _elementHelper.CurrentValue(questionId, viewModel, new FormAnswers(), "-month");
            var yearResult = _elementHelper.CurrentValue(questionId, viewModel, new FormAnswers(), "-year");

            // Assert
            Assert.Equal("14", dayResult);
            Assert.Equal("09", monthResult);
            Assert.Equal("2010", yearResult);
        }

        [Fact]
        public void CurrentValue_ReturnsEmptyStringWhenNoQuestionIdFound()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>();

            // Act
            var dayResult = _elementHelper.CurrentValue("question-id", viewModel, new FormAnswers(), "-day");
            var monthResult = _elementHelper.CurrentValue("question-id", viewModel, new FormAnswers(), "-month");
            var yearResult = _elementHelper.CurrentValue("question-id", viewModel, new FormAnswers(), "-year");

            // Assert
            Assert.Equal("", dayResult);
            Assert.Equal("", monthResult);
            Assert.Equal("", yearResult);
        }

        [Fact]
        public void ElementBuilder_ShouldReselectOnReturn_IfErroredElseWhere()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithQuestionId("questionId")
                .WithValue("option1")
                 .WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1"},
                  new Option { Value = "option2", Text = "Option 2"} })
                .Build();

            // Act
            _elementHelper.ReSelectPreviousSelectedOptions(element);

            // Assert
            Assert.True(element.Properties.Options[0].Selected);
            Assert.False(element.Properties.Options[1].Selected);
        }

        [Fact]
        public void ElementBuilder_ShouldRecheckRadioOnReturn_IfErroredElseWhere()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Radio)
                .WithQuestionId("questionId")
                .WithValue("option1")
                 .WithOptions(new List<Option>
                { new Option { Value = "option1", Text = "Option 1"},
                  new Option { Value = "option2", Text = "Option 2"} })
                .Build();

            // Act
            _elementHelper.ReCheckPreviousRadioOptions(element);

            // Assert
            Assert.True(element.Properties.Options[0].Checked);
            Assert.False(element.Properties.Options[1].Checked);
        }

        [Theory]
        [InlineData(EElementType.Address)]
        [InlineData(EElementType.Street)]
        public void ElementBuilder_ShouldThrowExceptionIfNoProviderGiven(EElementType type)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("Test")
                .Build();

            //Assert
            Assert.Throws<Exception>(() => _elementHelper.CheckForProvider(element));
        }

        [Theory]
        [InlineData(EElementType.Address)]
        [InlineData(EElementType.Street)]
        public void ElementBuilder_ShouldReturnTrue_IfProviderGiven(EElementType type)
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(type)
                .WithQuestionId("Test")
                .WithStreetProvider("FakeStreet")
                .WithAddressProvider("FakeAddress")
                .Build();

            //Assert
            Assert.True(_elementHelper.CheckForProvider(element));
        }

        [Fact]
        public async Task GenerateQuestionAndAnswersList_ShouldReturnFormSummary_WhenDataHas_PreviousAnswers()
        {
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
               .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers> { new Answers { QuestionId = "question", Response = "test answer" } } } } }));

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var element = new ElementBuilder()
                .WithQuestionId("question")
                .WithType(EElementType.Textbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = await _elementHelper.GenerateQuestionAndAnswersList("12345", formSchema);

            Assert.NotEmpty(result);
            Assert.Single(result);
        }

        [Theory]
        [InlineData(EElementType.Address, "Address Title")]
        [InlineData(EElementType.Street, "Street title")]
        [InlineData(EElementType.Organisation, "Organisation title")]
        public async Task GenerateQuestionAndAnswersList_ShouldReturnFormSummary_WithPageTitleAsLabel(EElementType type, string title)
        {
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(
                    new FormAnswers
                    {
                        Pages = new List<PageAnswers>
                        {
                            new PageAnswers
                            {
                                PageSlug = "page-one",
                                Answers = new List<Answers> { new Answers { QuestionId = "question", Response = "test answer" }}
                            }
                        },
                    }));

            _mockElementMapper
                .Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .ReturnsAsync("address");

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var element = new ElementBuilder()
                .WithQuestionId("question")
                .WithType(type)
                .WithAddressProvider("testProvider")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .WithPageTitle(title)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = await _elementHelper.GenerateQuestionAndAnswersList("12345", formSchema);

            Assert.NotNull(result[0].Answers[title]);
        }

        [Fact]
        public async Task GenerateQuestionAndAnswersList_ShouldReturnMultipleFormSummary_ForAddAnother()
        {
            _mockDistributedCacheWrapper.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(Newtonsoft.Json.JsonConvert.SerializeObject(
                    new FormAnswers
                    {
                        Pages = new List<PageAnswers>
                        {
                            new PageAnswers
                            {
                                PageSlug = "page-one",
                                Answers = new List<Answers> { new Answers { QuestionId = "question", Response = "test answer" }}
                            }
                        },
                        FormData = new Dictionary<string, object>
                        {
                            { $"{AddAnotherConstants.IncrementKeyPrefix}question", "1" }
                        },
                    }));

            _mockElementMapper
                .Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .ReturnsAsync("address");

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var element = new ElementBuilder()
                .WithQuestionId("question")
                .WithType(EElementType.AddAnother)
                .WithLabel("Add another label")
                .WithAddressProvider("testProvider")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithPageSlug("page-one")
                .WithBehaviour(behaviour)
                .WithPageTitle("Add another title")
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = await _elementHelper.GenerateQuestionAndAnswersList("12345", formSchema);

            var incrementedSummary = result.FirstOrDefault(_ => _.PageTitle.Equals("Add another label"));
            var overallPageSummary = result.FirstOrDefault(_ => _.PageTitle.Equals("Add another title"));

            Assert.NotNull(incrementedSummary);
            Assert.Equal("page-one", incrementedSummary.PageSlug);
            Assert.Equal("page-one-question-1", incrementedSummary.PageSummaryId);
            Assert.NotNull(overallPageSummary);
            Assert.Equal("page-one", overallPageSummary.PageSlug);
            Assert.Equal("page-one", overallPageSummary.PageSummaryId);
        }

        [Fact]
        public void GenerateValidDocumentUploadUrl_ShouldReturnUrlWithCaseRefEncoded()
        {
            //Arrange          
            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers
            {
                CaseReference = "12345"
            };

            var urlOrigin = $"https://www.test.com/";
            var caseReference = Convert.ToBase64String(Encoding.ASCII.GetBytes(formAnswers.CaseReference));
            var urlPath = $"{schema.BaseURL}/{FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH}{SystemConstants.CaseReferenceQueryString}{caseReference}";

            _mockElementHelper.Setup(_ => _.GenerateDocumentUploadUrl(It.IsAny<Element>(),
                It.IsAny<FormSchema>(),
                It.IsAny<FormAnswers>()
                )
            );

            var elementHelper = new ElementHelper(
                _mockDistributedCacheWrapper.Object,
                _mockElementMapper.Object,
                _mockWebHostEnvironment.Object,
                _mockHttpContextAccessor.Object
                );
            //Act
            var results = elementHelper.GenerateDocumentUploadUrl(new DocumentUpload(), schema, formAnswers);

            //Assert
            Assert.NotNull(results);
            Assert.Equal($"{urlOrigin}{urlPath}", results);
            Assert.Contains(caseReference, results);
        }

        [Fact]
        public void OrderOptionsAlphabetically_Should_OrderOptions_When_OrderOptionsAlphabetically_Is_True()
        {
            //Arrange          
            var options = new List<Option> {
                new Option {
                    Text = "X",
                    Value = "X"
                },
                new Option {
                    Text = "A",
                    Value = "A"
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithOptions(options)
                .WithOrderOptionsAlphabetically(true)
                .WithQuestionId("test")
                .Build();

            var elementHelper = new ElementHelper(
                _mockDistributedCacheWrapper.Object,
                _mockElementMapper.Object,
                _mockWebHostEnvironment.Object,
                _mockHttpContextAccessor.Object
                );
            //Act
            elementHelper.OrderOptionsAlphabetically(element);

            //Assert
            Assert.Equal("A", element.Properties.Options[0].Text);
            Assert.Equal("X", element.Properties.Options[1].Text);
        }

        [Fact]
        public void OrderOptionsAlphabetically_Should_Not_OrderOptions_When_OrderOptionsAlphabetically_Is_False()
        {
            //Arrange          
            var options = new List<Option> {
                new Option {
                    Text = "X",
                    Value = "X"
                },
                new Option {
                    Text = "A",
                    Value = "A"
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithOptions(options)
                .WithOrderOptionsAlphabetically(false)
                .WithQuestionId("test")
                .Build();

            var elementHelper = new ElementHelper(
                _mockDistributedCacheWrapper.Object,
                _mockElementMapper.Object,
                _mockWebHostEnvironment.Object,
                _mockHttpContextAccessor.Object
                );
            //Act
            elementHelper.OrderOptionsAlphabetically(element);

            //Assert
            Assert.Equal("X", element.Properties.Options[0].Text);
            Assert.Equal("A", element.Properties.Options[1].Text);
        }

        [Fact]
        public void GetAddAnotherNumberOfFieldsets_ShouldReturnIncrementId_WhenFormDataContainKey()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.AddAnother)
               .WithQuestionId("test-id")
               .WithLabel("test-text")
               .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                    PageSlug = "test-slug",
                    Answers = new List<Answers>{
                        new Answers {
                            QuestionId = "test-id",
                            Response = "this is the value"
                            }
                        }
                    }
                },
                FormData = new Dictionary<string, object>
                {
                    { $"{AddAnotherConstants.IncrementKeyPrefix}test-id", "1" }
                },
            };

            var result = _elementHelper.GetAddAnotherNumberOfFieldsets(element, formAnswers);

            Assert.IsType<int>(result);
        }

        [Fact]
        public void GetAddAnotherNumberOfFieldsets_ShouldReturnZero_WhenFormDataDoesnotContainKey()
        {
            // Arrange
            var element = new ElementBuilder()
               .WithType(EElementType.AddAnother)
               .WithQuestionId("test-id")
               .WithLabel("test-text")
               .Build();

            var formAnswers = new FormAnswers
            {
                Pages = new List<PageAnswers>
                {
                    new PageAnswers {
                    PageSlug = "test-slug",
                    Answers = new List<Answers>{
                        new Answers {
                            QuestionId = "test-id",
                            Response = "this is the value"
                            }
                        }
                    }
                },
            };

            var result = _elementHelper.GetAddAnotherNumberOfFieldsets(element, formAnswers);
            Assert.Equal(0, result);
        }
    }
}