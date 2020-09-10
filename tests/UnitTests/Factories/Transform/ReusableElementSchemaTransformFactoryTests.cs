using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Transforms.ReusableElements;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class ReusableElementSchemaTransformFactoryTests
    {
        private readonly Mock<IReusableElementTransformDataProvider> _transformDataProvider = new Mock<IReusableElementTransformDataProvider>();
        public ReusableElementSchemaTransformFactory ReusableElementSchemaTransformFactory;

        public ReusableElementSchemaTransformFactoryTests()
        {
        }

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_Element_ShouldBe_Replaced()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync(new Textbox
                {
                    Properties = new BaseProperty
                    {
                        QuestionId = "ReusableTest"
                    }
                });

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_transformDataProvider.Object);

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema()
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
            Assert.True(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Type == EElementType.Textbox);
            _transformDataProvider.Verify(_ => _.Get(It.Is<string>(x => x.Equals("test"))), Times.Once);
        }

        [Fact]
        public async Task Transform_ShouldThrowException_If_SubstituteNotFound()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync((IElement)null);

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_transformDataProvider.Object);
            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema()
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
            Assert.Equal("ReusableElementSchemaTransformFactory::CreateSubstituteRecord, No substitute element could be created for question ReusableTest", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldThrowException_If_ElementReference_NotFound()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync((IElement)null);

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_transformDataProvider.Object);
            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema()
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
            Assert.Equal("ReusableElementSchemaTransformFactory::CreateSubstituteRecord, no reusable element reference ID was specified", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldThrowException_If_QuestionIdNotFound()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync((IElement)null);

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_transformDataProvider.Object);
            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema()
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
            Assert.Equal("ReusableElementSchemaTransformFactory::CreateSubstituteRecord, no question ID was specified", result.Message);
        }

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_ShouldUpdate_TargetMapping_And_Optional_Properties()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync(new Textbox
                {
                    Properties = new BaseProperty
                    {
                        QuestionId = "ReusableTest"
                    }
                });

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_transformDataProvider.Object);

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            element.ElementRef = "test";
            element.Properties.Optional = true;
            element.Properties.TargetMapping = "target.mapping";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema()
            {
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

            // Assert
            Assert.IsType<FormSchema>(result);
            Assert.Equal("target.mapping", result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.TargetMapping);
            Assert.True(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Optional);
        }

        
        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_ShouldUpdate_MaxLength_WhenSupplied()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.IsAny<string>()))
                .ReturnsAsync(new Textbox
                {
                    Properties = new BaseProperty
                    {
                        QuestionId = "ReusableTest"
                    }
                });

            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_transformDataProvider.Object);

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("ReusableTest")
                .Build();

            element.ElementRef = "test";
            element.Properties.MaxLength = 30;

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema()
            {
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

            // Assert
            Assert.IsType<FormSchema>(result);
            Assert.Equal(30, result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.MaxLength);
        }
    }
}