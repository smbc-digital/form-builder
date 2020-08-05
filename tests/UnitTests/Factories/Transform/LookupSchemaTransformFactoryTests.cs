using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform;
using form_builder.Factories.Transform.Lookups;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Transforms.Lookups;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Schema
{
    public class LookupSchemaTransformFactoryTests
    {
        private readonly LookupSchemaTransformFactory _lookupSchemaTransformFactory;
        private readonly Mock<ILookupTransformDataProvider> _TransformDataProvider = new Mock<ILookupTransformDataProvider>();

        public LookupSchemaTransformFactory LookupSchemaTransformFactory => _lookupSchemaTransformFactory;

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

            var result = LookupSchemaTransformFactory.Transform(new FormSchema(){
                Pages = new List<Page>{
                    new Page()
                    {
                        Elements = new List<IElement>
                        {
                            element
                        }
                    }
                }
            });

            Assert.IsType<FormSchema>(result);
            Assert.Single(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options);
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

            var result = Assert.Throws<AggregateException>(() => LookupSchemaTransformFactory.Transform(new FormSchema(){
                Pages = new List<Page>{
                    new Page()
                    {
                        Elements = new List<IElement>
                        {
                            element
                        }
                    }
                }
            }));

            Assert.Equal($"LookupSchemaTransformFactory::Build, No lookup options found for question {element.Properties.QuestionId} with lookup value {element.Lookup}", result.InnerException.Message);
        }

        [Fact]
        public async Task Transform_ShouldJoin_CurrentOptions_WithOptions_FromDataSource()
        {
            var element = new ElementBuilder()
                .WithType(EElementType.Select)
                .WithLookup("lookup")
                .WithOptions(new List<Option>{ new Option { Value = "anotheroption" } })
                .Build();

            var result = LookupSchemaTransformFactory.Transform(new FormSchema(){
                Pages = new List<Page>{
                    new Page()
                    {
                        Elements = new List<IElement>
                        {
                            element
                        }
                    }
                }
            });

            Assert.Equal(2, result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options.Count);
            Assert.Equal("anotheroption", result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options[0].Value);
            Assert.Equal("test", result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Options[1].Value);
        }
    }
}
