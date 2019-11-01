using form_builder.Models;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class PageBuilder
    {
        private string _title = "TestTitle";
        private string _pageURL = "test-url";
        private List<Element> _elements = new List<Element>();
        private List<Behaviour> _behaviours = new List<Behaviour>();


        public Page Build()
        {
            return new Page
            {
                Title = _title,
                PageURL = _pageURL,
                Behaviours = _behaviours,
                Elements = _elements
            };
        }


        public PageBuilder WithPageTitle(string pageTitle)
        {
            _pageURL = pageTitle;

            return this;
        }

        public PageBuilder WithElement(Element element)
        {
            _elements.Add(element);

            return this;
        }
    }
}
