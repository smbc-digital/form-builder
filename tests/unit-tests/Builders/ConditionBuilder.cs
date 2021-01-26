using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder_tests.Builders
{
    public class ConditionBuilder
    {
        private List<Condition> _conditions = new List<Condition>();
        private ECondition _conditionType;
        private string _comparisonValue;
        private string _questionId;


        public Condition Build() => new Condition
        {
            Conditions = _conditions,
            ConditionType = _conditionType,
            ComparisonValue = _comparisonValue,
            QuestionId = _questionId
        };

        public ConditionBuilder WithConditionType(ECondition type)
        {
            _conditionType = type;

            return this;
        }

        public ConditionBuilder WithComparisonValue(string value)
        {
            _comparisonValue = value;

            return this;
        }

        public ConditionBuilder WithQuestionId(string value)
        {
            _questionId = value;

            return this;
        }

        public ConditionBuilder WithCondition(Condition condition)
        {
            _conditions.Add(condition);

            return this;
        }
    }
}