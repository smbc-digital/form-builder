using form_builder.Enum;
using form_builder.Models;
using System.Collections.Generic;
using Action = form_builder.Models.Actions.Action;

namespace form_builder_tests.Builders
{
    public class FormSchemaBuilder
    {
        private string _baseUrl = "base-url";
        private string _feedbackForm = "www.feedback.com";

        private string _feedbackPhase = "";
        private string _formName = "formname";
        private List<Page> _pages = new List<Page>();
        private string _startPageSlug = "page-one";
        private bool _documentDownload;
        private List<EDocumentType> _documentType = new List<EDocumentType>();
        private List<IAction> _formActions = new List<IAction>(); 

        private List<EnvironmentAvailability> _environmentAvailability = new List<EnvironmentAvailability>();

        public FormSchema Build()
        {
            return new FormSchema
            {
                BaseURL = _baseUrl,
                FeedbackForm = _feedbackForm,
                FeedbackPhase = _feedbackPhase,
                FormName = _formName,
                Pages = _pages,
                StartPageSlug = _startPageSlug,
                EnvironmentAvailabilities = _environmentAvailability,
                DocumentDownload = _documentDownload,
                DocumentType = _documentType,
                FormActions = _formActions
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

        public FormSchemaBuilder WithFeedback(string phase, string feedbackFormUrl)
        {
            _feedbackPhase = phase;
            _feedbackForm = feedbackFormUrl;
            return this;
        }

        public FormSchemaBuilder WithStartPageSlug(string slug)
        {
            _startPageSlug = slug;
            return this;
        }

        public FormSchemaBuilder WithDocumentDownload(bool value)
        {
            _documentDownload = value;
            return this;
        }

        public FormSchemaBuilder WithDocumentType(EDocumentType documentType)
        {
            _documentType.Add(documentType);
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

        public FormSchemaBuilder WithFormActions(IAction formAction)
        {
            _formActions.Add(formAction);

            return this;
        }
    }
}
