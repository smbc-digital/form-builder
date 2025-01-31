using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models.Actions;
using form_builder.Models.Elements;

namespace form_builder.Models
{
    public class FormSchema
    {
        public string FormName { get; set; }

        public string BaseURL { get; set; }

        public string StartPageUrl { get; set; }

        public string FirstPageSlug { get; set; }

        public string FormAccessKey { get; set; }
        
        public string FormAccessKeyName { get; set; }   

        public List<string> FormAccessReferrers { get; set; }
        
        public bool Embeddable { get; set; }

        public string FeedbackForm { get; set; }

        public string FeedbackPhase { get; set; }

        public bool GenerateReferenceNumber { get; set; }

        public string GeneratedReferenceNumberMapping { get; set; }

        public bool SavePaymentAmount { get; set; }

        public string PaymentAmountMapping { get; set; } = "paymentAmount";

        public bool ProcessPaymentCallbackResponse { get; set; }

        public string CallbackFailureContactNumber { get; set; }

        public string PaymentIssueButtonUrl { get; set; } = "https://www.stockport.gov.uk";

        public string PaymentIssueButtonLabel { get; set; } = "Go to the homepage";

        public string ReferencePrefix { get; set; }

        public List<Breadcrumb> BreadCrumbs { get; set; }

        public List<Page> Pages { get; set; }

        public List<IAction> FormActions { get; set; } = new List<IAction>();

        public List<EnvironmentAvailability> EnvironmentAvailabilities { get; set; }

        public bool HasDocumentUpload => Pages.Any(_ => _.PageSlug.Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH));

        public List<EDocumentType> DocumentType { get; set; }

        public FormSchema()
        {
            EnvironmentAvailabilities = new List<EnvironmentAvailability>();
        }

        public Page GetPage(IPageHelper pageHelper, string path, string form)
        {
            try
            {
                var pages = Pages.Where(_ => _.PageSlug.Trim().Equals(path.Trim(), StringComparison.OrdinalIgnoreCase)).OrderByDescending(_ => _.RenderConditions.Count).ToList();

                if (pages.Count.Equals(1))
                    return pages.First();

                var page = pageHelper.GetPageWithMatchingRenderConditions(pages, form);

                return page;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Requested path '{path}' object could not be found or was not unique.", ex);
            }
        }

        public IElement GetElement(string questionId) =>
            Pages.SelectMany(_ => _.Elements)
            .Where(_ => _.Properties.QuestionId is not null)
            .SingleOrDefault(_ => _.Properties.QuestionId.Equals(questionId));
    }
}