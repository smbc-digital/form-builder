using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Models;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class OptionalIfTransformFactoryTests
    {
        private readonly OptionalIfTransformFactory _transformFactory;
        private readonly Page _page;

        private readonly FormAnswers _savedAnswers = new()
        {
            Pages = new()
            {
                new PageAnswers
                {
                    Answers = new()
                    {
                        new Answers
                        {
                            QuestionId = "textbox",
                            Response = "test"
                        }
                    }
                }
            }
        };

        public OptionalIfTransformFactoryTests()
        {
            var textboxWithOptionalIf = new ElementBuilder()
                .WithQuestionId("textboxWithOptionalIf")
                .WithType(EElementType.Textbox)
                .WithOptionalIf("textbox")
                .WithOptionalIfValue("test")
                .WithOptional(false)
                .Build();

            var textboxWithOptionalNotIf = new ElementBuilder()
                .WithQuestionId("textboxWithOptionalNotIf")
                .WithType(EElementType.Textbox)
                .WithOptionalIf("textbox")
                .WithOptionalIfNotValue("test2")
                .WithOptional(false)
                .Build();

            var textbox = new ElementBuilder()
                .WithQuestionId("textbox")
                .WithType(EElementType.Textbox)
                .WithValue("test")
                .WithOptional(false)
                .Build();

            _page = new PageBuilder()
                .WithPageSlug("people")
                .WithElement(textboxWithOptionalIf)
                .WithElement(textboxWithOptionalNotIf)
                .WithElement(textbox)
                .Build();

            _transformFactory = new OptionalIfTransformFactory();
        }

        [Fact]
        public async Task Transform_ShouldReturnOptional_WithOptionalIfOnPage()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);

            // Assert
            Assert.Single(result.Elements.Where(_ => _.Properties.Optional && _.Properties.QuestionId.Equals("textboxWithOptionalIf")));
        }

        [Fact]
        public async Task Transform_ShouldReturnOptional_WithOptionalNotIfOnPage()
        {
            // Act
            var result = await _transformFactory.Transform(_page, _savedAnswers);

            // Assert
            Assert.Single(result.Elements.Where(_ => _.Properties.Optional && _.Properties.QuestionId.Equals("textboxWithOptionalNotIf")));
        }
    }
}
