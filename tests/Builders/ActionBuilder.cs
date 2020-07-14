using System;
using System.Linq;
using System.Reflection;
using form_builder.Enum;
using form_builder.Models.Properties.ActionProperties;
using Action = form_builder.Models.Actions.Action;

namespace form_builder_tests.Builders
{
    class ActionBuilder
    {
        private EActionType _type = EActionType.RetrieveExternalData;
        private BaseActionProperty _actionProperties = new BaseActionProperty();

        public IAction Build()
        {
              var elementType =  typeof(IAction).GetTypeInfo().Assembly
                .GetTypes()
                .Where(type => type.Name == _type.ToString())
                .FirstOrDefault();

            var element = (Action)Activator.CreateInstance(elementType);

            element.Properties = _actionProperties;
            return element;
        }

        public ActionBuilder WithActionType(EActionType type)
        {
            _type = type;
            return this;
        }

        public ActionBuilder WithTo(string to)
        {
            _actionProperties.To = to;
            return this;
        }
        
        public ActionBuilder WithFrom(string from)
        {
            _actionProperties.From = from;
            return this;
        }
        public ActionBuilder WithContent(string content)
        {
            _actionProperties.Content = content;
            return this;
        }

        public ActionBuilder WithSubject(string subject)
        {
            _actionProperties.Subject = subject;
            return this;
        }

        public ActionBuilder WithAuthToken(string authToken)
        {
            _actionProperties.AuthToken = authToken;
            return this;
        }

        public ActionBuilder WithTargetQuestionId(string targetQuestionId)
        {
            _actionProperties.TargetQuestionId = targetQuestionId;
            return this;
        }

        public ActionBuilder WithUrl(string url)
        {
            _actionProperties.URL = url;
            return this;
        }
    }
}