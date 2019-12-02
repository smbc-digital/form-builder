using form_builder.Enum;
using form_builder.Models;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class BehaviourBuilder
    {
        private List<Condition> _conditions = new List<Condition>();
        private EBehaviourType _behaviourType = EBehaviourType.GoToExternalPage;
        private string _PageSlug = "test-url";

        public Behaviour Build()
        {
            return new Behaviour
            {
                BehaviourType = _behaviourType,
                PageSlug = _PageSlug,
                Conditions = _conditions
            };
        }

        public BehaviourBuilder WithBehaviourType(EBehaviourType type)
        {
            _behaviourType = type;
            return this;
        }

        public BehaviourBuilder WithPageSlug(string PageSlug)
        {
            _PageSlug = PageSlug;
            return this;
        }

        public BehaviourBuilder WithCondition(Condition condition)
        {
            _conditions.Add(condition);
            return this;
        }
    }
}