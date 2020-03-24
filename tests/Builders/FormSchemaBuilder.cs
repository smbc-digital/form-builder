using form_builder.Models;
using System.Collections.Generic;

namespace form_builder_tests.Builders
{
    public class FormSchemaBuilder
    {
        private string _baseUrl = "base-url";
        private string _feedbackForm = "www.feedback.com";
        private string _formName = "formname";
        private List<Page> _pages = new List<Page>();
        private string _startPageSlug = "page-one";

        private List<EnvironmentAvailability> _environmentAvailability = new List<EnvironmentAvailability>();

        public FormSchema Build()
        {
            return new FormSchema
            {
                BaseURL = _baseUrl,
                FeedbackForm = _feedbackForm,
                FormName = _formName,
                Pages = _pages,
                StartPageSlug = _startPageSlug,
                EnvironmentAvailabilities = _environmentAvailability
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
            _formName = name;
            return this;
        }

        public FormSchemaBuilder WithStartPageSlug(string slug)
        {
            _startPageSlug = slug;
            return this;
        }

        public FormSchemaBuilder WithEnvironmentAvailability(string environment, bool isAvailable)
        {
            _environmentAvailability.Add(new EnvironmentAvailability{
                Environment = environment,
                IsAvailable = isAvailable
            });
            return this;
        }
    }
}
