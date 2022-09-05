using form_builder.Builders;
using form_builder.Enum;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Models;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Factories.Transform
{
    public class BehaviourConditionsPageTransformFactoryTests
    {
        private readonly BehaviourConditionsPageTransformFactory _transformFactory;
        private readonly Page _page;

        public BehaviourConditionsPageTransformFactoryTests()
        {
            var condition = new ConditionBuilder()
                .WithConditionType(ECondition.PaymentAmountEqualTo)
                .WithComparisonValue("0")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(condition)
                .Build();

            _page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            _transformFactory = new BehaviourConditionsPageTransformFactory();
        }

        [Fact]
        public async Task Transform_ShouldSetQuestionId_WhenTypeIs_PaymentAmountEqualTo_WhenQuestionIdNotSet()
        {
            // Act
            var result = await _transformFactory.Transform(_page, new FormAnswers());

            // Assert
            Assert.Equal("{{PAYMENTAMOUNT}}", result.Behaviours.First().Conditions.First().QuestionId);
        }

        [Fact]
        public async Task Transform_ShouldSetQuestionId_WhenTypeIs_PaymentAmountEqualTo_AndOverrideExistingValue()
        {
            // Arrange
            var condition = new ConditionBuilder()
                .WithConditionType(ECondition.PaymentAmountEqualTo)
                .WithQuestionId("fakeId")
                .WithComparisonValue("0")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithCondition(condition)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .Build();

            // Act
            var result = await _transformFactory.Transform(page, new FormAnswers());

            // Assert
            Assert.Equal("{{PAYMENTAMOUNT}}", result.Behaviours.First().Conditions.First().QuestionId);
        }
    }
}
