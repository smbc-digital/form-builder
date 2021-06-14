using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class AddAnotherSchemaTransformFactoryTests
    {
        private readonly AddAnotherSchemaTransformFactory _transformFactory = new AddAnotherSchemaTransformFactory();

        private readonly FormSchema _baseSchema = new FormSchema
        {
            Pages = new List<Page>
            {
                new Page
                {
                    Elements = new List<IElement>
                    {
                        new Element
                        {
                            Type = EElementType.AddAnother,
                            Properties = new BaseProperty
                            {
                                CurrentIncrementOfFieldsets = 1,
                                Label = "Person",
                                Elements = new List<IElement>
                                {
                                    new Element
                                    {
                                        Type = EElementType.Textbox,
                                        Properties = new BaseProperty
                                        {
                                            QuestionId = "textbox"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        [Fact]
        public void Transform_ShouldReturnCorrectNumberOfElementsOnPage()
        {
            // Act
            var result = _transformFactory.Transform(_baseSchema);
            var resultPage = result.Pages[0];

            // Assert
            Assert.Equal(10, result.Pages[0].Elements.Count);
        }

        [Fact]
        public void Transform_ShouldRetainOriginalAddnotherElement()
        {
            // Act
            var result = _transformFactory.Transform(_baseSchema);
            var resultPage = result.Pages[0];

            // Assert
            Assert.Single(resultPage.Elements.Where(_ => _.Type.Equals(EElementType.AddAnother)));
        }

        [Fact]
        public void Transform_ShouldReturnCorrectNumberOf_FieldsetOpenElements()
        {
            // Act
            var result = _transformFactory.Transform(_baseSchema);
            var resultPage = result.Pages[0];

            // Assert
            Assert.Equal(2, resultPage.Elements.Count(_ => _.Type.Equals(EElementType.FieldsetOpen)));
        }

        [Fact]
        public void Transform_ShouldReturnCorrectNumberOf_FieldsetCloseElements()
        {
            // Act
            var result = _transformFactory.Transform(_baseSchema);
            var resultPage = result.Pages[0];

            // Assert
            Assert.Equal(2, resultPage.Elements.Count(_ => _.Type.Equals(EElementType.FieldsetClose)));
        }

        [Fact]
        public void Transform_ShouldReturnCorrect_PostBackButtonElements()
        {
            // Act
            var result = _transformFactory.Transform(_baseSchema);
            var resultPage = result.Pages[0];
            var removeButtonZero = resultPage.Elements.Where(_ => _.Type.Equals(EElementType.PostbackButton) && _.Properties.Name.Equals("remove-0"));
            var removeButtonOne = resultPage.Elements.Where(_ => _.Type.Equals(EElementType.PostbackButton) && _.Properties.Name.Equals("remove-1"));
            var addAnotherButton = resultPage.Elements.Where(_ => _.Type.Equals(EElementType.PostbackButton) && _.Properties.Name.Equals("addAnotherFieldset"));

            // Assert
            Assert.Equal(3, resultPage.Elements.Count(_ => _.Type.Equals(EElementType.PostbackButton)));
            Assert.Single(removeButtonZero);
            Assert.Single(removeButtonOne);
            Assert.Single(addAnotherButton);
        }

        [Fact]
        public void Transform_ShouldReturnCorrect_TextboxElements()
        {
            // Act
            var result = _transformFactory.Transform(_baseSchema);
            var resultPage = result.Pages[0];
            var textboxZero = resultPage.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox:0:"));
            var textboxOne = resultPage.Elements.Where(_ => _.Type.Equals(EElementType.Textbox) && _.Properties.QuestionId.Equals("textbox:1:"));

            // Assert
            Assert.Equal(2, resultPage.Elements.Count(_ => _.Type.Equals(EElementType.Textbox)));
            Assert.Single(textboxZero);
            Assert.Single(textboxOne);
        }
    }
}
