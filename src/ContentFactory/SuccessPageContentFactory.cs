using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.Services.PageService.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace form_builder.ContentFactory
{
    public interface ISuccessPageContentFactory
    {
        Task<SuccessPageEntity> Build(string form, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers, EBehaviourType behaviourType);
    }

    public class SuccessPageContentFactory : ISuccessPageContentFactory
    {
        private readonly IPageHelper _pageHelper;
        private readonly IHostingEnvironment _enviroment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPageContentFactory _pageContentFactory;
        public SuccessPageContentFactory(IHttpContextAccessor httpContextAccessor, IHostingEnvironment enviroment, IPageHelper pageHelper, IPageContentFactory pageContentFactory)
        {
            _pageHelper = pageHelper;
            _enviroment = enviroment;
            _pageContentFactory = pageContentFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SuccessPageEntity> Build(string form, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers, EBehaviourType behaviourType)
        {
            var page = baseForm.GetPage("success");
            var startFormUrl = $"https://{_httpContextAccessor.HttpContext.Request.Host}/{baseForm.BaseURL}/{baseForm.StartPageSlug}";
            
            if(page == null && behaviourType == EBehaviourType.SubmitAndPay)
            {
                page = GenerateGenericPaymentPage();
                baseForm.Pages.Add(page);
            }   

            if(page == null && (_enviroment.EnvironmentName == "prod" || _enviroment.EnvironmentName == "stage"))
                throw new Exception($"SuccessPageContentFactory::Build, No success page configured for form {form}");

            if (page == null)
            {
                return new SuccessPageEntity
                {
                    ViewName = "Submit",
                    FormAnswers = formAnswers,
                    FeedbackFormUrl = baseForm.FeedbackForm,
                    FormName = baseForm.FormName,
                    StartFormUrl = startFormUrl
                };
            }

            if(baseForm.DocumentDownload && page != null)
            {
                    baseForm.DocumentType.ForEach((docType) => {
                        var element = new ElementBuilder()
                            .WithType(EElementType.DocumentDownload)
                            .WithLabel($"Download summary")
                            .WithSource($"/document/Summary/{docType}/{sessionGuid}")
                            .WithDocumentType(docType)
                            .Build();

                        page.Elements.Add(element);
                    });
                    var successIndex = baseForm.Pages.IndexOf(page);    
                    baseForm.Pages[successIndex] = page;
            }

            var result = await _pageContentFactory.Build(page, baseForm, sessionGuid);

            return new SuccessPageEntity
            {
                HtmlContent = result,
                FeedbackFormUrl = baseForm.FeedbackForm,
                FormName = baseForm.FormName,
                StartFormUrl = startFormUrl,
                PageTitle = page.Title
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
                .WithPropertyText("<a class=\"button-primary\" href=\"https://www.stockport.gov.uk\">Go to home page</a>")
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
    }
}
