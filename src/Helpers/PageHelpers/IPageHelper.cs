using form_builder.Models;
using form_builder.Services.PageService.Entities;
using form_builder.ViewModels;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        void HasDuplicateQuestionIDs(List<Page> pages, string formName);
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressSearchResults = null, List<OrganisationSearchResult> organisationSearchResults = null);
        void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid);
        Task<ProcessRequestEntity> ProcessOrganisationJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<OrganisationSearchResult> organisationResults);
        Task<ProcessRequestEntity> ProcessStreetJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults);
        Task<ProcessRequestEntity> ProcessAddressJourney(string journey, Page currentPage, Dictionary<string, dynamic> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults);
        void CheckForInvalidQuestionOrTargetMappingValue(List<Page> pages, string formName);
        Task CheckForPaymentConfiguration(List<Page> pages, string formName);
        void CheckForDocumentDownload(FormSchema formSchema);
        void CheckForEmptyBehaviourSlugs(List<Page> pages, string formName);
        void CheckForCurrentEnvironmentSubmitSlugs(List<Page> pages, string formName);
        void CheckSubmitSlugsHaveAllProperties(List<Page> pages, string formName);
        void CheckForAcceptedFileUploadFileTypes(List<Page> pages, string formName);
        void SaveFormData(string key, object value, string guid);
        Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, Dictionary<string, dynamic> formData);
        void CheckForIncomingFormDataValues(List<Page> Pages);
    }
}