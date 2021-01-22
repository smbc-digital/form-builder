using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
        public string FeedbackForm { get; set; }

        public string FeedbackPhase { get; set; }

        public List<Breadcrumb> BreadCrumbs { get; set; }

        public List<Page> Pages { get; set; }

        public List<IAction> FormActions { get; set; } = new List<IAction>();
        
        public List<EnvironmentAvailability> EnvironmentAvailabilities { get; set; }

        public bool DocumentDownload { get; set; }

        public bool HasDocumentUpload => Pages.Any(_ => _.PageSlug == FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH);

        public List<EDocumentType> DocumentType { get; set; }

        public FormSchema()
        {
            EnvironmentAvailabilities = new List<EnvironmentAvailability>();
        }

        public Page GetPage(IPageHelper pageHelper, string path)
        {
            try
            {
                var pages = Pages.Where(_ => _.PageSlug.ToLower().Trim() == path.ToLower().Trim()).OrderByDescending(_ => _.RenderConditions.Count).ToList();

                if (pages.Count == 1)
                    return pages.First();

                var page = pageHelper.GetPageWithMatchingRenderConditions(pages);

                return page;
            }
            catch(Exception ex)
            {
                throw new ApplicationException($"Requested path '{path}' object could not be found or was not unique.", ex);
            }
        }

        // TODO: Thbis needs tests
        public IElement GetElement(string questionId)
        {
            return Pages.SelectMany(_ => _.Elements)
                    .SingleOrDefault(_ => _.Properties.QuestionId == questionId);

        }        

        // TODO: This needs tests
        public bool HasElement(string questionId)
        {
            return Pages.SelectMany(_ => _.Elements)
                    .Any(_ => _.Properties.QuestionId == questionId);
        }

        public async Task ValidateFormSchema(IPageHelper pageHelper, string form, string path)
        {
            if (path != FirstPageSlug)
                return;

            pageHelper.HasDuplicateQuestionIDs(Pages, form);
            pageHelper.CheckForEmptyBehaviourSlugs(Pages, form);
            pageHelper.CheckForInvalidQuestionOrTargetMappingValue(Pages, form);
            pageHelper.CheckForCurrentEnvironmentSubmitSlugs(Pages, form);
            pageHelper.CheckSubmitSlugsHaveAllProperties(Pages, form);
            await pageHelper.CheckForPaymentConfiguration(Pages, form);
            pageHelper.CheckForAcceptedFileUploadFileTypes(Pages, form);
            pageHelper.CheckConditionalElementsAreValid(Pages, form);
            pageHelper.CheckForDocumentDownload(this);
            pageHelper.CheckForIncomingFormDataValues(Pages);
            pageHelper.CheckForPageActions(this);
            pageHelper.CheckRenderConditionsValid(Pages);
            pageHelper.CheckAddressNoManualTextIsSet(Pages);
            pageHelper.CheckForAnyConditionType(Pages);
            pageHelper.CheckUploadedFilesSummaryQuestionsIsSet(Pages);
            pageHelper.CheckForBookingElement(Pages);
        }

        public bool IsAvailable(string environment)
        {
            var environmentAvailability = EnvironmentAvailabilities.SingleOrDefault(_ => _.Environment.ToLower().Equals(environment.ToLower()));
            return environmentAvailability == null || environmentAvailability.IsAvailable;
        }
    }
}