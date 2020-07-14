using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public List<Page> Pages { get; set; }
        
        public List<EnvironmentAvailability> EnvironmentAvailabilities { get; set; }

        public bool DocumentDownload { get; set; }
        
        public List<EDocumentType> DocumentType { get; set; }

        public FormSchema()
        {
            EnvironmentAvailabilities = new List<EnvironmentAvailability>();
        }

        public Page GetPage(string path)
        {
            Page page;
            try
            {
                page = Pages.SingleOrDefault(_ => _.PageSlug.ToLower().Trim() == path.ToLower().Trim());
            }
            catch(Exception ex)
            {
                throw new ApplicationException($"Requested path '{path}' object could not be found or was not unique.", ex);
            }
            
            return page;
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
            pageHelper.CheckForDocumentDownload(this);
            pageHelper.CheckForIncomingFormDataValues(Pages);
        }

        public bool IsAvailable(string environment)
        {
            var environmentAvailability = EnvironmentAvailabilities.SingleOrDefault(_ => _.Environment.ToLower().Equals(environment.ToLower()));
            if (environmentAvailability == null)
            {
                return true;
            }

            return environmentAvailability.IsAvailable;
        }
    }
}