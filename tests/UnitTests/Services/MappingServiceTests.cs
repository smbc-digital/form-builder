using form_builder.Builders;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService;
using form_builder_tests.Builders;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.FileManagement;
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
        private readonly Mock<ICache> _mockCache = new Mock<ICache>();
        private readonly Mock<IOptions<DistributedCacheExpirationConfiguration>> _mockDistrbutedCacheExpirationConfiguration = new Mock<IOptions<DistributedCacheExpirationConfiguration>>();

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

            _mockDistrbutedCacheExpirationConfiguration.Setup(_ => _.Value).Returns(new DistributedCacheExpirationConfiguration
            {
                FormJson = 1
            });

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(schema);

            _service = new MappingService( _mockDistrubutedCache.Object, _mockElementMapper.Object, _mockCache.Object, _mockDistrbutedCacheExpirationConfiguration.Object);
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
        public async Task Map_ShouldCallCache_ToGetFormSchema()
        {

            // Act
            await _service.Map("form", "guid");

            // Assert
            _mockCache.Verify(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>
                (It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()), Times.Once);
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

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(schema);

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            Assert.Empty(resultData as IDictionary<string, object>);
        }

        [Fact]
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

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(schema); ;

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(new { });

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;

            Assert.NotNull(castResultsData.text);
        }

        [Fact]
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

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(new { });

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
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

        [Fact]
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

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(new { });

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
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

        [Fact]
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

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
               .Returns(JsonConvert.SerializeObject(new FormAnswers
               {
                   Pages = new List<PageAnswers>()
               }));

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(new { });

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

        
        [Fact]
        public async Task Map_ShouldReturnEmptyExpandoObject_WhenNullResponse_ForFile()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("file")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
               .Returns(JsonConvert.SerializeObject(new FormAnswers
               {
                   Pages = new List<PageAnswers>()
               }));

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(null);

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;

            Assert.Empty(castResultsData);
        }

                
        [Fact]
        public async Task Map_ShouldReturnExpandoObject_WithSingleFile()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("file")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("filetwo")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
               .Returns(JsonConvert.SerializeObject(new FormAnswers
               {
                   Pages = new List<PageAnswers>()
               }));

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.Is<IElement>(x => x.Properties.QuestionId == "file"), It.IsAny<FormAnswers>()))
                .Returns(null);

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
                .Returns(new File());

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;

            Assert.Single(castResultsData.file);
            Assert.NotNull(castResultsData);
        }

        [Fact]
        public async Task Map_ShouldReturnExpandoObject_WithTwoFiles_WithSameMapping()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("file")
                .WithTargetMapping("file")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("filetwo")
                .WithTargetMapping("file")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.FileUpload)
                .WithQuestionId("filethree")
                .WithTargetMapping("file")
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

            _mockCache.Setup(_ => _.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ESchemaType>()))
                .ReturnsAsync(schema);

            _mockDistrubutedCache.Setup(_ => _.GetString(It.IsAny<string>()))
               .Returns(JsonConvert.SerializeObject(new FormAnswers
               {
                   Pages = new List<PageAnswers>()
               }));

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.Is<IElement>(x => x.Properties.QuestionId == "file"), It.IsAny<FormAnswers>()))
                .Returns(new File());

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.Is<IElement>(x => x.Properties.QuestionId == "filetwo"), It.IsAny<FormAnswers>()))
                .Returns(null);

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.Is<IElement>(x => x.Properties.QuestionId == "filethree"), It.IsAny<FormAnswers>()))
                .Returns(new File());

            // Act
            var result = await _service.Map("form", "guid");

            // Assert
            var resultData = Assert.IsType<ExpandoObject>(result.Data);
            dynamic castResultsData = resultData;

            Assert.Equal(2,castResultsData.file.Count);
            Assert.NotNull(castResultsData);
            Assert.NotNull(castResultsData.file);
        }
    }
}
