using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Microsoft.AspNetCore.Hosting;

namespace form_builder.ContentFactory.SuccessPageFactory
{
    public class SuccessPageFactory : ISuccessPageFactory
    {
        private readonly IPageHelper _pageHelper;
        private readonly IPageFactory _pageFactory;
        private readonly ISessionHelper _sessionHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IWebHostEnvironment _environment;
        public SuccessPageFactory(IPageHelper pageHelper, IPageFactory pageFactory,
            ISessionHelper sessionHelper, IDistributedCacheWrapper distributedCache,
            IWebHostEnvironment environment)
        {
            _pageHelper = pageHelper;
            _pageFactory = pageFactory;
            _sessionHelper = sessionHelper;
            _distributedCache = distributedCache;
            _environment = environment;
        }

        public async Task<SuccessPageEntity> Build(string form, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers, EBehaviourType behaviourType)
        {
            var page = baseForm.GetPage(_pageHelper, "success");

            _distributedCache.Remove(sessionGuid);
            _sessionHelper.RemoveSessionGuid();

            if (page is null && behaviourType.Equals(EBehaviourType.SubmitAndPay))
            {
                page = GenerateGenericPaymentPage();
                baseForm.Pages.Add(page);
            }

            if (page is null)
            {
                return new SuccessPageEntity
                {
                    ViewName = "Submit",
                    FormAnswers = formAnswers,
                    CaseReference = formAnswers.CaseReference,
                    FeedbackFormUrl = baseForm.FeedbackForm,
                    FeedbackPhase = baseForm.FeedbackPhase,
                    FormName = baseForm.FormName,
                    StartPageUrl = baseForm.StartPageUrl
                };
            }

            if (baseForm.DocumentDownload)
            {
                var sourceBase = _environment.EnvironmentName.Equals("local") ? "/document/Summary" : "/v2/document/Summary";
                baseForm.DocumentType.ForEach(docType =>
                {
                    var element = new ElementBuilder()
                        .WithType(EElementType.DocumentDownload)
                        .WithLabel($"Download {docType} document")
                        .WithSource($"{sourceBase}/{docType}/{sessionGuid}")
                        .WithDocumentType(docType)
                        .Build();

                    page.Elements.Add(element);
                });
                var successIndex = baseForm.Pages.IndexOf(page);
                baseForm.Pages[successIndex] = page;
            }

            var result = await _pageFactory.Build(page, new Dictionary<string, dynamic>(), baseForm, sessionGuid, formAnswers);

            return new SuccessPageEntity
            {
                HtmlContent = result.RawHTML,
                CaseReference = formAnswers.CaseReference,
                FeedbackFormUrl = result.FeedbackForm,
                FeedbackPhase = result.FeedbackPhase,
                FormName = result.FormName,
                StartPageUrl = result.StartPageUrl,
                PageTitle = result.PageTitle,
                BannerTitle = page.BannerTitle,
                LeadingParagraph = page.LeadingParagraph,
                DisplayBreadcrumbs = page.DisplayBreadCrumbs,
                Breadcrumbs = baseForm.BreadCrumbs,
                IsInPreviewMode = result.IsInPreviewMode
            };
        }

        public async Task<SuccessPageEntity> BuildBooking(string form, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers)
        {
            _sessionHelper.RemoveSessionGuid();

            var page = GenerateGenericBookingPage(baseForm);

            var result = await _pageFactory.Build(page, new Dictionary<string, dynamic>(), baseForm, sessionGuid, formAnswers);

            return new SuccessPageEntity
            {
                HtmlContent = result.RawHTML,
                CaseReference = formAnswers.CaseReference,
                FeedbackFormUrl = result.FeedbackForm,
                FeedbackPhase = result.FeedbackPhase,
                FormName = result.FormName,
                StartPageUrl = result.StartPageUrl,
                PageTitle = result.PageTitle,
                BannerTitle = page.BannerTitle,
                LeadingParagraph = page.LeadingParagraph,
                DisplayBreadcrumbs = page.DisplayBreadCrumbs,
                Breadcrumbs = baseForm.BreadCrumbs,
                IsInPreviewMode = result.IsInPreviewMode
            };
        }

        private Page GenerateGenericPaymentPage()
        {
            var h2Element = new ElementBuilder()
                .WithType(EElementType.H2)
                .WithPropertyText("Thank you for your payment")
                .Build();

            var pElement = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("Please keep a record of these details as you will need them if you want to contact us about your payment")
                .Build();

            var pElement2 = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("If you provided your email address when you made your payment, you will receive an email receipt.")
                .Build();

            var h2Element2 = new ElementBuilder()
                .WithType(EElementType.H2)
                .WithPropertyText("What happens next")
                .Build();

            var pElement3 = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("Content author: You should include information that is helpful to the user about what they need to do next.")
                .Build();

            var aElement = new ElementBuilder()
                .WithType(EElementType.P)
                .WithPropertyText("<a class=\"govuk-button\" href=\"https://www.stockport.gov.uk\">Go to home page</a>")
                .Build();

            return new Page
            {
                Elements = new List<IElement>
                {
                    h2Element,
                    pElement,
                    pElement2,
                    h2Element2,
                    pElement3,
                    aElement
                },
                PageSlug = "success"
            };
        }

        private Page GenerateGenericBookingPage(FormSchema baseForm)
        {
            var linkElement = new ElementBuilder()
                .WithType(EElementType.Link)
                .WithOpenInTab(true)
                .WithUrl("https://www.stockport.gov.uk")
                .WithPropertyText("Go to home page")
                .WithClassName("govuk-button")
                .Build();

            return new Page
            {
                Elements = new List<IElement>
                {
                    linkElement
                },
                PageSlug = "booking-cancel-success",
                BannerTitle = "You've successfully cancelled your appointment",
                LeadingParagraph = "We've received your cancellation request",
                Title = "Success",
                HideTitle = true,
                DisplayBreadCrumbs = baseForm.BreadCrumbs is not null && baseForm.BreadCrumbs.Any()
            };
        }
    }
}
