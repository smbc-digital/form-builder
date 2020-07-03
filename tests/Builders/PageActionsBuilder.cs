using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Properties;

namespace form_builder_tests.Builders
{
    class PageActionsBuilder
    {
        private EPageActionType _type = EPageActionType.RetrieveExternalData;
        private BaseProperty _properties = new BaseProperty();

        public PageAction Build()
        {
            return new PageAction
            {
                Type = _type,
                Properties = _properties
            };
        }

        public PageActionsBuilder WithActionType(EPageActionType type)
        {
            _type = type;
            return this;
        }

        public PageActionsBuilder WithProperties(BaseProperty properties)
        {
            _properties = properties;
            return this;
        }
    }
}
