using form_builder.Enum;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using form_builder_tests.Builders;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class MappingServiceTests
    {
        private readonly MappingService _service;
        private readonly Mock<ISchemaProvider> _mockSchemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistrubutedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<IElementMapper> _mockElementMapper = new Mock<IElementMapper>();

        public MappingServiceTests()
        {
            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("test-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new FormAnswers
                {
                    Pages = new List<PageAnswers>()
                }));

            _service = new MappingService(_mockSchemaProvider.Object, _mockDistrubutedCache.Object, _mockElementMapper.Object);
        }

        [Fact]
        public async Task Map_ShouldCallCacheProvider_ToGetFormData()
        {

            // Act
            await _service.Map("form", "guid");

            // Assert
            _mockDistrubutedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Map_ShouldCallSchemaProvider_ToGetFormSchema()
        {
            // Act
            await _service.Map("form", "guid");

            // Assert
            _mockSchemaProvider.Verify(_ => _.Get<FormSchema>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Map_ShouldReturnEmptyExpandoObject_WhenFormContains_NoValidatableElements()
        {
            var element = new ElementBuilder()
                 .WithType(EElementType.H1)
                 .WithQuestionId("test-question")
                 .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Img)
                .WithQuestionId("test-img")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Button)
                .WithButtonId("test-button")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithElement(element3)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            Assert.Empty(resultData as IDictionary<string, object>);
        }

        [Fact(Skip = "cause")]
        public async Task Map_ShouldReturnExpandoObject_WhenFormContains_SingleValidatableElement()
        {
            var element = new ElementBuilder()
                 .WithType(EElementType.H1)
                 .WithQuestionId("test-question")
                 .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("text")
                .Build();


            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;

            Assert.NotNull(castResultsData.text);
        }

        [Fact(Skip ="cause")]
        public async Task Map_ShouldReturnExpandoObject_WhenFormContains_MultipleValidatableElementsWithTargetMapping()
        {
            var element = new ElementBuilder()
                 .WithType(EElementType.H1)
                 .WithQuestionId("test-question")
                 .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("text")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test")
                .WithTargetMapping("customer.datepicker.date")
                .Build();


            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithElement(element3)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;
            var customerObject = castResultsData.customer;

            Assert.NotNull(castResultsData.text);
            Assert.NotNull(castResultsData.customer);
            Assert.NotNull(customerObject.datepicker);
        }

        [Fact(Skip = "cause")]
        public async Task Map_ShouldReturnExpandoObject_WhenFormContains_MultipleValidatableElementsWithTargetMapping_WithValues()
        {
            var elementOneAnswer = "text answer";
            var elementTwoAnswer = "01/01/2000";

            var element = new ElementBuilder()
                 .WithType(EElementType.H1)
                 .WithQuestionId("test-question")
                 .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("text")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.DatePicker)
                .WithQuestionId("test")
                .WithTargetMapping("customer.datepicker.date")
                .Build();


            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithElement(element3)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new FormAnswers
                {
                    Pages = new List<PageAnswers>
                    {
                        new PageAnswers
                        {
                            Answers = new List<Answers>
                            {
                                new Answers
                                {
                                    QuestionId = "text",
                                    Response = elementOneAnswer
                                },
                                new Answers
                                {
                                    QuestionId = "test",
                                    Response = elementTwoAnswer
                                }
                            }
                        }
                    }
                }));

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;
            var customerObject = castResultsData.customer;
            var datepickerObject = customerObject.datepicker;

            Assert.NotNull(castResultsData.customer);
            Assert.NotNull(customerObject.datepicker);

            Assert.Equal(elementOneAnswer, (string)castResultsData.text);
            Assert.Equal($"{elementTwoAnswer} 00:00:00", ((DateTime)datepickerObject.date).ToString());
        }

        [Fact(Skip = "cause")]
        public async Task Map_ShouldReturnExpandoObject_WhenFormContains_MultipleValidatableElementsWithSameTargetMapping()
        {
            var elementOneAnswer = "text answer";
            var elementTwoHours = "03";
            var elementTwoMinutes = "30";
            var elementTwoAmPm = "am";

            var hourId = "test-hours";
            var minuteId = "test-minutes";
            var amPmId = "test-ampm";

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("text")
                .WithTargetMapping("customer.textboxtime")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId("test")
                .WithTargetMapping("customer.textboxtime")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
                .Returns(JsonConvert.SerializeObject(new FormAnswers
                {
                    Pages = new List<PageAnswers>
                    {
                        new PageAnswers
                        {
                            Answers = new List<Answers>
                            {
                                new Answers
                                {
                                    QuestionId = "text",
                                    Response = elementOneAnswer
                                },
                                new Answers
                                {
                                    QuestionId = hourId,
                                    Response = elementTwoHours
                                },
                                new Answers
                                {
                                    QuestionId = minuteId,
                                    Response = elementTwoMinutes
                                },
                                new Answers
                                {
                                    QuestionId = amPmId,
                                    Response = elementTwoAmPm
                                }
                            }
                        }
                    }
                }));

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;
            var customerObject = castResultsData.customer;

            Assert.NotNull(customerObject.textboxtime);

            Assert.Equal($"{elementOneAnswer} {elementTwoHours}:{elementTwoMinutes}:00", (string)customerObject.textboxtime);
        }

        [Fact(Skip = "cause")]
        public async Task Map_ShouldReturnExpandoObject_WhenFormContains_ValidatableElementWithComplexTargetMapping()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping("one.two.three.four.five.six")
                .WithQuestionId("text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
               .Returns(JsonConvert.SerializeObject(new FormAnswers
               {
                   Pages = new List<PageAnswers>()
               }));

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;
            var oneObject = castResultsData.one;
            var twoObject = oneObject.two;
            var threeObject = twoObject.three;
            var fourObject = threeObject.four;
            var fiveObject = fourObject.five;

            Assert.NotNull(oneObject);
            Assert.NotNull(twoObject);
            Assert.NotNull(threeObject);
            Assert.NotNull(fourObject);
            Assert.NotNull(fiveObject);
            Assert.NotNull(fiveObject.six);
        }

        [Fact(Skip = "cause")]
        public async Task Map_ShouldReturnExpandoObject_WhenFormContains_MutipleValidatableElementsWithComplexTargetMapping()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithTargetMapping("one.two.three.four.five.six")
                .WithQuestionId("text")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithTargetMapping("one.two.three.four.value")
                .WithQuestionId("text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockSchemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
               .Returns(JsonConvert.SerializeObject(new FormAnswers
               {
                   Pages = new List<PageAnswers>()
               }));

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;
            var oneObject = castResultsData.one;
            var twoObject = oneObject.two;
            var threeObject = twoObject.three;
            var fourObject = threeObject.four;
            var fiveObject = fourObject.five;

            Assert.NotNull(oneObject);
            Assert.NotNull(twoObject);
            Assert.NotNull(threeObject);
            Assert.NotNull(fourObject);
            Assert.NotNull(fourObject.value);
            Assert.NotNull(fiveObject);
            Assert.NotNull(fiveObject.six);
        }
    }
}
