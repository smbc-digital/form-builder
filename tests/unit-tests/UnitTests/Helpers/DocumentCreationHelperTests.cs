using System.Collections.Generic;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.DocumentCreation;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class DocumentCreationHelperTests
    {
        private readonly DocumentCreationHelper _documentCreation;
        private readonly Mock<IElementMapper> _mockElementMapper = new();

        public DocumentCreationHelperTests()
        {
            _documentCreation = new DocumentCreationHelper(_mockElementMapper.Object);
        }

        [Fact]
        public void GenerateQuestionAndAnswersList_ShouldReturn_Summary_With_SingleAnswer_And_NewLine()
        {
            // Arrange
            _mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns("test value");
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };

            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("QuestionId")
                .WithLabel("I am a label")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GenerateQuestionAndAnswersList_ShouldRemove_OtherPages_NotRelated_ToJourney_WhenBuilding_Answers()
        {
            // Arrange
            _mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns("test value");
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-two", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-three", Answers = new List<Answers>() } } };

            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("QuestionIdOne")
                .WithLabel("I am a label")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("QuestionIdtwo")
                .WithLabel("I am a label")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("QuestionIdthree")
                .WithLabel("I am a label")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-three")
                .Build();

            var condition = new ConditionBuilder()
                .WithConditionType(ECondition.EqualTo)
                .WithQuestionId("QuestionIdOne")
                .WithComparisonValue("orange")
                .Build();

            var behaviour2 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(condition)
                .WithPageSlug("page-two")
                .Build();

            var behaviour3 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("page-three")
                .Build();

            var behaviour4 = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .WithBehaviour(behaviour)
                .WithBehaviour(behaviour2)
                .Build();

            var page2 = new PageBuilder()
                .WithPageSlug("page-two")
                .WithElement(element2)
                .WithBehaviour(behaviour3)
                .Build();

            var page3 = new PageBuilder()
                .WithPageSlug("page-three")
                .WithElement(element3)
                .WithBehaviour(behaviour4)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithPage(page2)
                .WithPage(page3)
                .Build();

            // Act
            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void GenerateQuestionAndAnswersList_ShouldNot_Add_NewLines_IfAnswer_Returned_Was_Null_Or_Empty()
        {
            // Arrange
            var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };

            var element = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("QuestionId")
                .WithLabel("I am a label")
                .Build();

            var element2 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("QuestionIdTwo")
                .WithLabel("I am a label")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("QuestionIdThree")
                .WithLabel("I am a label")
                .Build();

            var element4 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("QuestionIdFour")
                .WithLabel("I am a label")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithPageSlug("page-one")
                .WithElement(element)
                .WithElement(element2)
                .WithElement(element3)
                .WithElement(element4)
                .WithBehaviour(behaviour)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            // Act
            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Empty(result);
        }
    }
}