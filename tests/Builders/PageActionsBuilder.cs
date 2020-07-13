using System;
using System.Linq;
using System.Reflection;
using form_builder.Enum;
using form_builder.Models.Properties.ActionProperties;
using Action = form_builder.Models.Actions.Action;

namespace form_builder_tests.Builders
{
    class PageActionsBuilder
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

        public PageActionsBuilder WithActionType(EActionType type)
        {
            _type = type;
            return this;
        }

        public PageActionsBuilder WithActionProperties(BaseActionProperty actionProperties)
        {
            _actionProperties = actionProperties;
            return this;
        }
    }
}