using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Transforms.ReusableElements;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class ReusableElementSchemaTransformFactoryTests
    {
        private readonly Mock<IReusableElementTransformDataProvider> _transformDataProvider = new();
        public ReusableElementSchemaTransformFactory ReusableElementSchemaTransformFactory;

        public ReusableElementSchemaTransformFactoryTests()
        {
            ReusableElementSchemaTransformFactory = new ReusableElementSchemaTransformFactory(_transformDataProvider.Object);
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
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
            Assert.True(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Type.Equals(EElementType.Textbox));
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
            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
            var result = await Assert.ThrowsAsync<Exception>(() => ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
                .WithOptional(true)
                .WithTargetMapping("target.mapping")
                .WithSetAutofocus(true)
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
            Assert.True(result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.SetAutofocus);
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
                .WithMaxLength(30)
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema
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

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_ShouldUpdate_MinLength_WhenSupplied()
        {
            // Arrange
            _transformDataProvider
                .Setup(mock => mock.Get(It.IsAny<string>()))
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
                .WithMinLength(30)
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema
            {
                Pages = new List<Page> 
                {
                    new()
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
            Assert.Equal(30, result.Pages.First().Elements.First().Properties.MinLength);
        }

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_ShouldUpdate_SummaryLabel_WhenSupplied()
        {
            // Arrange
            var summaryLabel = "custom summary label";
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
                .WithSummaryLabel(summaryLabel)
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
            Assert.Equal(summaryLabel, result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.SummaryLabel);
        }

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_ShouldUpdate_Hint_WhenSupplied()
        {
            // Arrange
            var hintText = "hint text";
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
                .WithHint(hintText)
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
            Assert.Equal(hintText, result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.Hint);
        }

        [Fact]
        public async Task Transform_ShouldCall_TransformDataProvider_And_ShouldUpdate_CustomValidationMessage_WhenSupplied()
        {
            // Arrange
            var validationMessage = "custom validation message";
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
                .WithCustomValidationMessage(validationMessage)
                .Build();

            element.ElementRef = "test";

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(new FormSchema
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
            Assert.Equal(validationMessage, result.Pages.FirstOrDefault().Elements.FirstOrDefault().Properties.CustomValidationMessage);
        }

        [Fact]
        public async Task Transform_ShouldTransform_Correct_Elements_WhenMultiplePagesExists_WithSameSlug()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.Is<string>(_ => _ == "elementRef1")))
                .ReturnsAsync(new ElementBuilder().WithType(EElementType.Textbox).WithQuestionId("baseQuestionIdRef1").Build());

            _transformDataProvider
                .Setup(_ => _.Get(It.Is<string>(_ => _ == "elementRef2")))
                .ReturnsAsync(new ElementBuilder().WithType(EElementType.Textbox).WithQuestionId("baseQuestionIdRef2").Build());

            _transformDataProvider
                .Setup(_ => _.Get(It.Is<string>(_ => _ == "elementRef3")))
                .ReturnsAsync(new ElementBuilder().WithType(EElementType.Textbox).WithQuestionId("baseQuestionIdRef3").Build());

            _transformDataProvider
                .Setup(_ => _.Get(It.Is<string>(_ => _ == "elementRef4")))
                .ReturnsAsync(new ElementBuilder().WithType(EElementType.Textbox).WithQuestionId("baseQuestionIdRef4").Build());

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test1")
                .Build();
            element.ElementRef = "elementRef1";

            var element2 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test2")
                .Build();
            element2.ElementRef = "elementRef2";

            var element3 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test3")
                .Build();
            element3.ElementRef = "elementRef3";

            var element4 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test4")
                .Build();
            element4.ElementRef = "elementRef4";

            var pageOne = new PageBuilder()
                .WithPageSlug("duplicate")
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var pageTwo = new PageBuilder()
                .WithPageSlug("duplicate")
                .WithElement(element3)
                .WithElement(element4)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(pageOne)
                .WithPage(pageTwo)
                .Build();

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(formSchema);

            // Assert
            Assert.IsType<FormSchema>(result);
            Assert.Equal("test1", result.Pages[0].Elements[0].Properties.QuestionId);
            Assert.Equal("test2", result.Pages[0].Elements[1].Properties.QuestionId);
            Assert.Equal("test3", result.Pages[1].Elements[0].Properties.QuestionId);
            Assert.Equal("test4", result.Pages[1].Elements[1].Properties.QuestionId);
        }

        [Fact]
        public async Task Transform_ShouldTransform_ElementsRef_WhenUsedMultipleTimes_AcrossPages()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.Is<string>(_ => _ == "elementRef1")))
                .ReturnsAsync(() => new ElementBuilder().WithType(EElementType.Textbox).WithQuestionId("baseQuestionIdRef1").Build());

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test1")
                .Build();
            element.ElementRef = "elementRef1";

            var element2 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test2")
                .Build();
            element2.ElementRef = "elementRef1";

            var element3 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test3")
                .Build();
            element3.ElementRef = "elementRef1";

            var pageOne = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .Build();

            var pageTwo = new PageBuilder()
                .WithPageSlug("page-two")
                .WithElement(element2)
                .Build();

            var pageThree = new PageBuilder()
                .WithPageSlug("page-three")
                .WithElement(element3)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(pageOne)
                .WithPage(pageTwo)
                .WithPage(pageThree)
                .Build();

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(formSchema);

            // Assert
            Assert.IsType<FormSchema>(result);
            Assert.Equal("test1", result.Pages[0].Elements[0].Properties.QuestionId);
            Assert.Equal("test2", result.Pages[1].Elements[0].Properties.QuestionId);
            Assert.Equal("test3", result.Pages[2].Elements[0].Properties.QuestionId);
        }

        [Fact]
        public async Task Transform_ShouldTransform_Correct_Elements_WhenMultiplePagesExists_WithSameSlug_AndSameElementReference()
        {
            // Arrange
            _transformDataProvider
                .Setup(_ => _.Get(It.Is<string>(_ => _ == "elementRef1")))
                .ReturnsAsync(() => new ElementBuilder().WithType(EElementType.Textbox).WithQuestionId("baseQuestionIdRef1").Build());

            _transformDataProvider
                .Setup(_ => _.Get(It.Is<string>(_ => _ == "elementRef2")))
                .ReturnsAsync(() => new ElementBuilder().WithType(EElementType.Radio).WithQuestionId("baseQuestionIdRef2").Build());

            var element = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test1")
                .Build();
            element.ElementRef = "elementRef1";

            var element2 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test2")
                .Build();
            element2.ElementRef = "elementRef2";

            var element3 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test3")
                .Build();
            element3.ElementRef = "elementRef1";

            var element4 = (Reusable)new ElementBuilder()
                .WithType(EElementType.Reusable)
                .WithQuestionId("test4")
                .Build();

            element4.ElementRef = "elementRef2";

            var pageOne = new PageBuilder()
                .WithPageSlug("duplicate")
                .WithElement(element)
                .WithElement(element2)
                .Build();

            var pageTwo = new PageBuilder()
                .WithPageSlug("duplicate")
                .WithElement(element3)
                .WithElement(element4)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(pageOne)
                .WithPage(pageTwo)
                .Build();

            // Act
            var result = await ReusableElementSchemaTransformFactory.Transform(formSchema);

            // Assert
            Assert.IsType<FormSchema>(result);
            Assert.Equal("test1", result.Pages[0].Elements[0].Properties.QuestionId);
            Assert.Equal("test2", result.Pages[0].Elements[1].Properties.QuestionId);
            Assert.Equal("test3", result.Pages[1].Elements[0].Properties.QuestionId);
            Assert.Equal("test4", result.Pages[1].Elements[1].Properties.QuestionId);
        }
    }
}