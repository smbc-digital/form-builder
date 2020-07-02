using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Services.RetrieveExternalDataService;

namespace form_builder.Workflows
{
    public interface IActionsWorkflow
    {
        Task Process(Page page, string formName);
    }

    public class ActionsWorkflow : IActionsWorkflow
    {
        private readonly IRetrieveExternalDataService _retrieveExternalDataService;
        public ActionsWorkflow(IRetrieveExternalDataService retrieveExternalDataService)
        {
            _retrieveExternalDataService = retrieveExternalDataService;
        }

        public async Task Process(Page page, string formName)
        {
            if (page.PageActions.Any(_ => _.Type.Equals(EPageActionType.RetrieveExternalData)))
                await _retrieveExternalDataService.Process(page.PageActions.Where(_ => _.Type == EPageActionType.RetrieveExternalData).ToList(), formName);
        }
    }
}
