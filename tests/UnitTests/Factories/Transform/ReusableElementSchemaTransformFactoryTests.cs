using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Transforms.ReusableElements;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Schema
{
    public class ReusableElementSchemaTransformFactoryTests
    {

        private readonly Mock<IReusableElementTransformDataProvider> _TransformDataProvider = new Mock<IReusableElementTransformDataProvider>();
        public ReusableElementSchemaTransformFactory ReusableElementSchemaTransformFactory;

        public ReusableElementSchemaTransformFactoryTests()
        {

        }

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_Element_ShouldBe_Replaced()
        {
            _TransformDataProvider.Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync(new Textbox
                {
                    Properties = new BaseProperty
                    {
                        QuestionId = "ReusableTest"
                    }
                });

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_TransformDataProvider.Object);

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            element.ElementRef = "test";

            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema(){
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
            Assert.True(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Type == EElementType.Textbox);
            _TransformDataProvider.Verify(_ => _.Get(It.Is<string>(x => x == "test")), Times.Once);
        }

        [Fact]
        public async Task Transform_ShouldThrowException_If_SubstituteNotFound()
        {
            _TransformDataProvider.Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync((IElement) null);

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_TransformDataProvider.Object);
            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            element.ElementRef = "test";

            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema(){
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

            Assert.Equal("ReusableElementSchemaTransformFactory::CreateSubstituteRecord, No subsitute element could be created for question ReusableTest", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldThrowException_If_ElementReference_NotFound()
        {
            _TransformDataProvider.Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync((IElement) null);

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_TransformDataProvider.Object);
            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema(){
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

            Assert.Equal("ReusableElementSchemaTransformFactory::CreateSubstituteRecord, no resusable element reference ID was specified", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldThrowException_If_QuestionIdNotFound()
        {
            _TransformDataProvider.Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync((IElement) null);

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_TransformDataProvider.Object);
            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .Build();

            element.ElementRef = "test";

            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema(){
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

            Assert.Equal("ReusableElementSchemaTransformFactory::CreateSubstituteRecord, no question ID was specified", result.Message);
        }

        
        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_ShouldUpdate_TargetMapping_And_Optional_Properties()
        {
            _TransformDataProvider.Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync(new Textbox
                {
                    Properties = new BaseProperty
                    {
                        QuestionId = "ReusableTest"
                    }
                });

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_TransformDataProvider.Object);

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            element.ElementRef = "test";
            element.Properties.Optional = true;
            element.Properties.TargetMapping = "target.mapping";

            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema(){
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
            Assert.Equal(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.TargetMapping, "target.mapping");
            Assert.True(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Optional);
        }
    }
}