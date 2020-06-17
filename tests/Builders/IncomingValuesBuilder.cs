using form_builder.Models;

namespace form_builder_tests.Builders
{
    public class IncomingValuesBuilder
    {
        private string _questionId;
        private string _name;
        private bool _optional;

        public IncomingValue Build()
        {
            return new IncomingValue
            {
                QuestionId = _questionId,
                Name = _name,
                Optional = _optional
            };
        }

        public IncomingValuesBuilder WithQuestionId(string questionId)
        {
            _questionId = questionId;

            return this;
        }

        public IncomingValuesBuilder WithName(string name)
        {
            _name = name;

            return this;
        }

        public IncomingValuesBuilder WithOptional(bool optional)
        {
            _optional = optional;

            return this;
        }
    }
}