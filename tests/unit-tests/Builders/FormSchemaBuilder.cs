﻿using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Actions;

namespace form_builder_tests.Builders
{
    public class FormSchemaBuilder
    {
        private string _baseUrl = "base-url";
        private string _feedbackForm = "www.feedback.com";
        private string _feedbackPhase = "";
        private string _formName = "formname";
        private string _keyValue  { get; set; }
        private string _keyName { get; set; }
        private List<Page> _pages = new List<Page>();

        private List<string> _referrers = new List<string>();
        private string _startPageUrl = "page-url";
        private string _firstPageSlug = "page-one";
        private bool _generateReferenceNumber = false;
        public string _generatedReferenceNumberMapping { get; set; }
        private bool _savePaymentAmount { get; set; }
        private string _paymentAmountMapping { get; set; }
        public string _referencePrefix { get; set; }
        private List<EDocumentType> _documentType = new();
        private List<IAction> _formActions = new();
        private List<EnvironmentAvailability> _environmentAvailability = new();
        private List<Breadcrumb> _breadcrumbs = new();
        private bool _processPaymentCallbackResponse = false;
        private string _callbackFailureContactNumber = "01611111111";

        public FormSchema Build() => new FormSchema
        {
            BaseURL = _baseUrl,
            FeedbackForm = _feedbackForm,
            FeedbackPhase = _feedbackPhase,
            FormName = _formName,
            Referrers = _referrers,
            Pages = _pages,
            StartPageUrl = _startPageUrl,
            FirstPageSlug = _firstPageSlug,
            EnvironmentAvailabilities = _environmentAvailability,
            DocumentType = _documentType,
            FormActions = _formActions,
            GenerateReferenceNumber = _generateReferenceNumber,
            ReferencePrefix = _referencePrefix,
            GeneratedReferenceNumberMapping = _generatedReferenceNumberMapping,
            BreadCrumbs = _breadcrumbs,
            SavePaymentAmount = _savePaymentAmount,
            PaymentAmountMapping = _paymentAmountMapping,
            ProcessPaymentCallbackResponse = _processPaymentCallbackResponse,
            CallbackFailureContactNumber = _callbackFailureContactNumber,
            Key = _keyValue,
            KeyName = _keyName
        };

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

        public FormSchemaBuilder WithStartPageUrl(string url)
        {
            _startPageUrl = url;

            return this;
        }

        public FormSchemaBuilder WithFirstPageSlug(string slug)
        {
            _firstPageSlug = slug;

            return this;
        }

        public FormSchemaBuilder WithDocumentType(EDocumentType documentType)
        {
            _documentType.Add(documentType);

            return this;
        }

        public FormSchemaBuilder WithEnvironmentAvailability(string environment, bool isAvailable)
        {
            _environmentAvailability.Add(new EnvironmentAvailability
            {
                Environment = environment,
                IsAvailable = isAvailable
            });

            return this;
        }

        public FormSchemaBuilder WithEnvironmentAvailability(string environment, bool isAvailable, List<EnabledForBase> enabedFor)
        {
            _environmentAvailability.Add(new EnvironmentAvailability
            {
                Environment = environment,
                IsAvailable = isAvailable,
                EnabledFor = enabedFor
            });

            return this;
        }

        public FormSchemaBuilder WithFormActions(IAction formAction)
        {
            _formActions.Add(formAction);

            return this;
        }

        public FormSchemaBuilder WithGeneratedReference(string mapping, string prefix)
        {
            _generateReferenceNumber = true;
            _generatedReferenceNumberMapping = mapping;
            _referencePrefix = prefix;

            return this;
        }

        public FormSchemaBuilder WithBreadcrumbs(List<Breadcrumb> breadcrumbs)
        {
            _breadcrumbs = breadcrumbs;

            return this;
        }

        public FormSchemaBuilder WithSavePaymentAmount(string mapping)
        {
            _savePaymentAmount = true;
            _paymentAmountMapping = mapping;

            return this;
        }

        public FormSchemaBuilder WithProcessPaymentCallbackResponse()
        {
            _processPaymentCallbackResponse = true;

            return this;
        }

        public FormSchemaBuilder WithSpecifiedKey(string keyName, string keyValue)
        {
            _keyName = keyName;
            _keyValue = keyValue;

            return this;
        }
        public FormSchemaBuilder WithKey(string keyValue)
        {
            _keyValue = keyValue;

            return this;
        }

        public FormSchemaBuilder WithKeyName(string keyName)
        {
            _keyName = keyName;
        
            return this;
        }

        
        public FormSchemaBuilder AddReferrer(string referrer)
        {
            _referrers.Add(referrer);
    
            return this;
        }

    }
}