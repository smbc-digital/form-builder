using form_builder.Helpers.PageHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Models
{
    public class FormSchema
    {
        public string FormName { get; set; }
        public string BaseURL { get; set; }
        public string StartPageSlug { get; set; }
        public string FeedbackForm { get; set; }
        public List<Page> Pages { get; set; }

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

        public void ValidateFormSchema(IPageHelper pageHelper, string form, string path)
        {
            if (path != StartPageSlug)
                return;

            pageHelper.hasDuplicateQuestionIDs(Pages, form);
            pageHelper.CheckForEmptyBehaviourSlugs(Pages, form);
            pageHelper.CheckForInvalidQuestionOrTargetMappingValue(Pages, form);
            pageHelper.CheckForPaymentConfiguration(Pages, form);
        }
    }
}