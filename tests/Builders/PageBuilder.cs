﻿using form_builder.Models;
using form_builder.Models.Elements;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class PageBuilder
    {
        private string _title = "TestTitle";
        private string _PageSlug = "test-url";
        private List<IElement> _elements = new List<IElement>();
        private List<Behaviour> _behaviours = new List<Behaviour>();


        public Page Build()
        {
            return new Page
            {
                Title = _title,
                PageSlug = _PageSlug,
                Behaviours = _behaviours,
                Elements = _elements
            };
        }

        public PageBuilder WithPageTitle(string title)
        {
            _title = title;

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
    }
}
