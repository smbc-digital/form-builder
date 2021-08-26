using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Element
{
    public class CheckboxElementCheckTests
    {
        private readonly CheckboxElementCheck _integrityCheck = new();

        [Theory]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        public void Validate_ShouldReturn_Valid_Result_WhenType_IsNotCheckbox(EElementType elementType)
        {
            var element = new ElementBuilder()
                .WithType(elementType)
                .Build();

           var result = _integrityCheck.Validate(element);

           Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_Valid_Result_When_Checkbox_DoesNot_ContainsAny_Exclsuive_Options()
        {
            var options = new List<Option>{
                new Option
                {
                    Value = "test",
                    Text = "test"
                },
                new Option {
                    Value = "numbertwo",
                    Text = "numbertwo"
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithOptions(options)
                .Build();

           var result = _integrityCheck.Validate(element);

           Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturn_InValid_Result_When_Checkbox_Contains_Multiple_ExsclsuvieOptions()
        {
            var questionId = "testQuestionId";
            var options = new List<Option>{
                new Option
                {
                    Value = "test",
                    Text = "test",
                    Exclusive = true
                },
                new Option {
                    Value = "numbertwo",
                    Text = "numbertwo"
                },
                new Option {
                    Value = "numbertwo",
                    Text = "numbertwo",
                    Exclusive = true
                }
            };

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Checkbox)
                .WithExclusiveCheckboxValidationMessage("error message")
                .WithOptions(options)
                .Build();

           var result = _integrityCheck.Validate(element);

           Assert.False(result.IsValid);
           Assert.Single(result.Messages);
           Assert.Collection<string>(result.Messages, message => Assert.Equal($"{IntegrityChecksConstants.FAILURE}Checkbox Element Check: {questionId} contains multiple options found with exclusive set to 'true', only a single one can be exclusive", message));
        }

        [Fact]
        public void Validate_ShouldReturn_InValid_Result_When_Checkbox_Contains_Missing_ValidationMessage_When_Exclsuive_Option_Is_Provide()
        {
            var questionId = "testQuestionId";
            var options = new List<Option>{
                new Option
                {
                    Value = "test",
                    Text = "test",
                    Exclusive = true
                },
                new Option {
                    Value = "numbertwo",
                    Text = "numbertwo"
                },
                new Option {
                    Value = "numbertwo",
                    Text = "numbertwo"
                }
            };

            var element = new ElementBuilder()
                .WithQuestionId(questionId)
                .WithType(EElementType.Checkbox)
                .WithOptions(options)
                .Build();

           var result = _integrityCheck.Validate(element);

           Assert.False(result.IsValid);
           Assert.Single(result.Messages);
           Assert.Collection<string>(result.Messages, message => Assert.Equal($"{IntegrityChecksConstants.FAILURE}Checkbox Element Check: You must provide a validation message when you have options which are exclsuive, Set 'ExclusiveCheckboxValidationMessage' property within element with questionId {questionId}", message));
        }
    }
}
