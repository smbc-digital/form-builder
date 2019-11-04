using form_builder.Models;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class FormSchemaBuilder
    {
        private string _baseUrl = "base-url";
        private string _feedbackForm = "www.feedback.com";
        private string _name = "formname";
        private List<Page> _pages = new List<Page>();
        private string _startPage = "page-one";

        public FormSchema Build()
        {
            return new FormSchema
            {
                BaseURL = _baseUrl,
                FeedbackForm = _feedbackForm,
                Name = _name,
                Pages = _pages,
                StartPage = _startPage
            };
        }

        public FormSchemaBuilder WithBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
            return this;
        }

        public FormSchemaBuilder WithPage(Page page)
        {
            _pages.Add(page);
            return this;
        }

        public FormSchemaBuilder WithName(string name)
        {
            _name = name;
            return this;
        }
    }
}
