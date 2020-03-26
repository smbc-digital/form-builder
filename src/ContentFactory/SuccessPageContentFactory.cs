using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.Services.PageService.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace form_builder.ContentFactory
{
    public class SuccessPageContentFactory
    {
        private readonly IPageHelper _pageHelper;
        private readonly IHostingEnvironment _enviroment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SuccessPageContentFactory(IHttpContextAccessor httpContextAccessor, IHostingEnvironment enviroment, IPageHelper pageHelper)
        {
            _pageHelper = pageHelper;
            _enviroment = enviroment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SuccessPageEntity> Build(string form, FormSchema baseForm, string sessionGuid, FormAnswers formAnswers)
        {
            var page = baseForm.GetPage("success");
            var startFormUrl = $"https://{_httpContextAccessor.HttpContext.Request.Host}/{baseForm.BaseURL}/{baseForm.StartPageSlug}";
            
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

            if(baseForm.DocumentDownload && page != null){
                    baseForm.DocumentType.ForEach((docType) => {
                        var element = new DocumentDownload
                        {
                            Properties = new BaseProperty {
                                Label = $"Download {docType} Document",
                                DocumentType = docType,
                                Source = $"/document/Summary/{docType}/{sessionGuid}"
                            }
                        };

                        page.Elements.Add(element);
                    });
                    var successIndex = baseForm.Pages.IndexOf(page);    
                    baseForm.Pages[successIndex] = page;
            }

            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, dynamic>(), baseForm, sessionGuid);           

            return new SuccessPageEntity
            {
                HtmlContent = viewModel.RawHTML,
                FeedbackFormUrl = baseForm.FeedbackForm,
                FormName = baseForm.FormName,
                StartFormUrl = startFormUrl
            };
        }

    }
}
