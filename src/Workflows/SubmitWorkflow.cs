using form_builder.Helpers.Session;
using form_builder.Models.Elements;
using form_builder.Models.Properties;
using form_builder.Services.MappingService;
using form_builder.Services.SubmitService.Entities;
using form_builder.Services.SubmtiService;
using System;
using System.Threading.Tasks;

namespace form_builder.Workflows
{
    public interface ISubmitWorkflow
    {
        Task<SubmitServiceEntity> Submit(string form);
    }

    public class SubmitWorkflow : ISubmitWorkflow
    {
        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly ISessionHelper _sessionHelper;

        public SubmitWorkflow(ISubmitService submitService, IMappingService mappingService, ISessionHelper sessionHelper)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
        }

        public async Task<SubmitServiceEntity> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                throw new ApplicationException($"A Session GUID was not provided.");
            }

            var data = await _mappingService.Map(sessionGuid, form);

            //TODO: Move this somewhere else
            if(data.BaseForm.DocumentDownload){
                var page = data.BaseForm.GetPage("success");
                
                if(page != null){
                    data.BaseForm.DocumentType.ForEach((docType) => {
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
                    var successIndex = data.BaseForm.Pages.IndexOf(page);
                    data.BaseForm.Pages[successIndex] = page;
                }
            }

            return await _submitService.ProcessSubmission(data, form, sessionGuid);
        }
    }
}
