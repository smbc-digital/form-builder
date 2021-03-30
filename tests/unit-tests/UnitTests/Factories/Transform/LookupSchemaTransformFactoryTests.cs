using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.Lookups;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Transforms.Lookups;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class LookupSchemaTransformFactoryTests
    {
        private readonly Mock<ILookupTransformDataProvider> _transformDataProvider = new ();

        private LookupSchemaTransformFactory LookupSchemaTransformFactory { get; }

        public LookupSchemaTransformFactoryTests()
        {
            _transformDataProvider
                .Setup(_ => _.Get<List<Option>>(It.IsAny<string>()))
                .ReturnsAsync(new List<Option> { new Option { Value = "test" } });

            LookupSchemaTransformFactory = new LookupSchemaTransformFactory(_transformDataProvider.Object);
        }

        [Fact]
        public void Transform_ShouldCall_TransformDataProvider()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithLookup("lookup")
                .Build();

            // Act
            var result = LookupSchemaTransformFactory.Transform(new FormSchema
            {
                Pages = new List<Page>{
                    new Page
                    {
                        Elements = new List<IElement>
                        {
                            element
                        }
                    }
                }
            });

            // Assert
            Assert.IsType<FormSchema>(result);
            Assert.Single(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options);
            _transformDataProvider.Verify(_ => _.Get<List<Option>>(It.Is<string>(x => x.Equals("lookup"))), Times.Once);
        }

        [Fact]
        public void Transform_ShouldThrowException_WhenNoOptions_ReturnedFrom_DataProvider()
        {
            // Arrange
            _transformDataProvider.Setup(_ => _.Get<List<Option>>(It.IsAny<string>()))
                .ReturnsAsync(new List<Option>());

            var element = new ElementBuilder().WithType(EElementType.Select)
                .WithLookup("lookup")
                .WithQuestionId("testid")
                .Build();

            // Act
            var result = Assert.Throws<AggregateException>(() => LookupSchemaTransformFactory.Transform(new FormSchema()
            {
                Pages = new List<Page>{
                    new Page
                    {
                        Elements = new List<IElement>
                        {
                            element
                        }
                    }
                }
            }));

            // Assert
            Assert.Equal($"LookupSchemaTransformFactory::Build, No lookup options found for question {element.Properties.QuestionId} with lookup value {element.Lookup}", result.InnerException.Message);
        }

        [Fact]
        public void Transform_ShouldJoin_CurrentOptions_WithOptions_FromDataSource()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithLookup("lookup")
                .WithOptions(new List<Option> { new Option { Value = "anotheroption" } })
                .Build();

            // Act
            var result = LookupSchemaTransformFactory.Transform(new FormSchema()
            {
                Pages = new List<Page>{
                    new Page
                    {
                        Elements = new List<IElement>
                        {
                            element
                        }
                    }
                }
            });

            // Assert
            Assert.Equal(2, result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options.Count);
            Assert.Equal("anotheroption", result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options[0].Value);
            Assert.Equal("test", result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options[1].Value);
        }
    }
}