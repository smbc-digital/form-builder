using form_builder.Models;
using form_builder.Models.Elements;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class PageBuilder
    {
        private string _title = "TestTitle";
        private string _PageSlug = "test-url";
        private bool _isValidated = false;
        private List<IElement> _elements = new List<IElement>();
        private List<Behaviour> _behaviours = new List<Behaviour>();
        private List<IncomingValue> _incomingValues = new List<IncomingValue>();
        private List<PageAction> _pageActions = new List<PageAction>();

        public Page Build()
        {
            return new Page
            {
                Title = _title,
                PageSlug = _PageSlug,
                IsValidated = _isValidated,
                Behaviours = _behaviours,
                Elements = _elements,
                IncomingValues = _incomingValues,
                PageActions = _pageActions
            };
        }

        public PageBuilder WithPageTitle(string title)
        {
            _title = title;

            return this;
        }

        public PageBuilder WithValidatedModel(bool value)
        {
            _isValidated = value;

            return this;
        }

        public PageBuilder WithPageSlug(string url)
        {
            _PageSlug = url;

            return this;
        }

        public PageBuilder WithElement(Element element)
        {
            _elements.Add(element);

            return this;
        }

        public PageBuilder WithBehaviour(Behaviour behaviour)
        {
            _behaviours.Add(behaviour);

            return this;
        }

        public PageBuilder WithIncomingValue(IncomingValue value)
        {
            _incomingValues.Add(value);

            return this;
        }

        public PageBuilder WithPageActions(PageAction pageAction)
        {
            _pageActions.Add(pageAction);

            return this;
        }
    }
}
