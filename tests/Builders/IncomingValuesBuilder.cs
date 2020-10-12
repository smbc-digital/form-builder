using form_builder.Enum;
using form_builder.Models;

namespace form_builder_tests.Builders
{
    public class IncomingValuesBuilder
    {
        private string _questionId;
        private string _name;
        private bool _optional;
        private EHttpActionType _httpActionType;
        private bool _base64Encoded;

        public IncomingValue Build() => new IncomingValue
        {
            QuestionId = _questionId,
            Name = _name,
            Optional = _optional,
            HttpActionType = _httpActionType,
            Base64Encoded = _base64Encoded
        };

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

        public IncomingValuesBuilder WithHttpActionType(EHttpActionType value)
        {
            _httpActionType = value;

            return this;
        }

        public IncomingValuesBuilder WithBase64Encoding(bool value)
        {
            _base64Encoded = value;

            return this;
        }
    }
}