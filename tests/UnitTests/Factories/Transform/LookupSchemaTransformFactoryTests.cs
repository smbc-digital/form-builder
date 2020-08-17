using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.TransformDataProvider;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Schema
{
    public class LookupSchemaTransformFactoryTests
    {
        private readonly LookupSchemaTransformFactory _lookupSchemaTransformFactory;
        private readonly Mock<ITransformDataProvider> _TransformDataProvider = new Mock<ITransformDataProvider>();

        public LookupSchemaTransformFactoryTests()
        {
            _TransformDataProvider.Setup(_ => _.Get<List<Option>>(It.IsAny<string>()))
                .ReturnsAsync(new List<Option>{ new Option { Value = "test" } });

            _lookupSchemaTransformFactory = new LookupSchemaTransformFactory(_TransformDataProvider.Object);
        }

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithLookup("lookup")
                .Build();

            var result = await _lookupSchemaTransformFactory.Transform<IElement>(element);
            Assert.IsType<Select>(result);
            Assert.Single(result.Properties.Options);
            _TransformDataProvider.Verify(_ => _.Get<List<Option>>(It.Is<string>(x => x == "lookup")), Times.Once);
        }

        [Fact]
        public async Task Transform_ShouldThrowException_WhenNoOptions_ReturnedFrom_DataProvider()
        {
            _TransformDataProvider.Setup(_ => _.Get<List<Option>>(It.IsAny<string>()))
                .ReturnsAsync(new List<Option>());

            var element = new ElementBuilder().WithType(EElementType.Select)
                .WithLookup("lookup")
                .WithQuestionId("testid")
                .Build();

            var result = await Assert.ThrowsAsync<Exception>(() => _lookupSchemaTransformFactory.Transform<IElement>(element));
            Assert.Equal($"LookupSchemaTransformFactory::Build, No lookup options found for question {element.Properties.QuestionId} with lookup value {element.Lookup}", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldJoin_CurrentOptions_WithOptions_FromDataSource()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithLookup("lookup")
                .WithOptions(new List<Option>{ new Option { Value = "anotheroption" } })
                .Build();

            var result = await _lookupSchemaTransformFactory.Transform<IElement>(element);

            Assert.Equal(2, result.Properties.Options.Count);
            Assert.Equal("anotheroption", result.Properties.Options[0].Value);
            Assert.Equal("test", result.Properties.Options[1].Value);
        }
    }
}
