using System.Collections.Generic;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Models.Elements;

namespace form_builder.Builders
{
    public class PageBuilder
    {
        private string _title = "TestTitle";
        private bool _hideTitle = false;
        private bool _hideBackButton = false;
        private string _pageSlug = "test-url";
        private bool _isValidated = false;
        private List<IElement> _elements = new List<IElement>();
        private List<Behaviour> _behaviours = new List<Behaviour>();
        private List<IncomingValue> _incomingValues = new List<IncomingValue>();
        private List<IAction> _pageActions = new List<IAction>();
        private List<Condition> _renderConditions = new List<Condition>();

        public Page Build()
        {
            return new Page
            {
                Title = _title,
                HideTitle = _hideTitle,
                HideBackButton = _hideBackButton,
                PageSlug = _pageSlug,
                IsValidated = _isValidated,
                Behaviours = _behaviours,
                Elements = _elements,
                IncomingValues = _incomingValues,
                PageActions = _pageActions,
                RenderConditions = _renderConditions
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
            _pageSlug = url;

            return this;
        }

        public PageBuilder WithElement(Element element)
        {
            _elements.Add(element);

            return this;
        }

        public PageBuilder WithHideTitle(bool value)
        {
            _hideTitle = value;

            return this;
        }

        public PageBuilder WithHideBackButton(bool value)
        {
            _hideBackButton = value;

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

        public PageBuilder WithPageActions(IAction pageAction)
        {
            _pageActions.Add(pageAction);

            return this;
        }

        public PageBuilder WithRenderConditions(Condition renderCondition)
        {
            _renderConditions.Add(renderCondition);

            return this;
        }
    }
}