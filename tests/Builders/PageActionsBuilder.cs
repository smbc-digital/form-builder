using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Properties.ActionProperties;

namespace form_builder_tests.Builders
{
    class PageActionsBuilder
    {
        private EPageActionType _type = EPageActionType.RetrieveExternalData;
        private BaseActionProperty _actionProperties = new BaseActionProperty();

        public PageAction Build()
        {
            return new PageAction
            {
                Type = _type,
                Properties = _actionProperties
            };
        }

        public PageActionsBuilder WithActionType(EPageActionType type)
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