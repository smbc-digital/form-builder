using form_builder.Enum;
using form_builder.Models;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class BehaviourBuilder
    {
        private List<Condition> _conditions = new List<Condition>();
        private EBehaviourType _behaviourType = EBehaviourType.GoToExternalPage;
        private string _PageSlug;
        private List<SubmitSlug> _submitSlugs = new List<SubmitSlug>();



        public Behaviour Build()
        {
            return new Behaviour
            {
                BehaviourType = _behaviourType,
                PageSlug = _PageSlug,
                Conditions = _conditions,
                SubmitSlugs = _submitSlugs
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

        public BehaviourBuilder WithSubmitSlug(SubmitSlug submitSlug)
        {
            _submitSlugs.Add(submitSlug);
            return this;
        }

        public BehaviourBuilder WithEmptySubmitSlug(SubmitSlug submitSlug)
        {
            _submitSlugs.Add(submitSlug);
            return this;
        }

        public BehaviourBuilder WithCondition(Condition condition)
        {
            _conditions.Add(condition);
            return this;
        }
    }
}