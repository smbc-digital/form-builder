using form_builder.Enum;
using form_builder.Models;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class BehaviourBuilder
    {
        private List<Condition> _conditions = new List<Condition>();
        private EBehaviourType _behaviourType = EBehaviourType.GoToExternalPage;
        private string _pageURL = "test-url";

        public Behaviour Build()
        {
            return new Behaviour
            {
                BehaviourType = _behaviourType,
                pageURL = _pageURL,
                Conditions = _conditions
            };
        }

        public BehaviourBuilder WithBehaviourType(EBehaviourType type)
        {
            _behaviourType = type;
            return this;
        }

        public BehaviourBuilder WithPageUrl(string pageUrl)
        {
            _pageURL = pageUrl;
            return this;
        }
    }
}