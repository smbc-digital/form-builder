﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Models;
using form_builder.Services.EmailService;
using form_builder.Services.RetrieveExternalDataService;

namespace form_builder.Workflows.ActionsWorkflow
{
    public interface IActionsWorkflow
    {
        Task Process(List<IAction> actions, FormSchema formSchema, string formName);
    }

    public class ActionsWorkflow : IActionsWorkflow
    {
        private readonly IRetrieveExternalDataService _retrieveExternalDataService;
        private readonly IEmailService _emailService;
        private readonly ISchemaFactory _schemaFactory;

        public ActionsWorkflow(IRetrieveExternalDataService retrieveExternalDataService, IEmailService emailService, ISchemaFactory schemaFactory)
        {
            _retrieveExternalDataService = retrieveExternalDataService;
            _emailService = emailService;
            _schemaFactory = schemaFactory;
        }

        public async Task Process(List<IAction> actions, FormSchema formSchema, string formName)
        {
            if(formSchema == null)
                formSchema = await _schemaFactory.Build(formName);

            if (actions.Any(_ => _.Type.Equals(EActionType.RetrieveExternalData)))
                await _retrieveExternalDataService.Process(actions.Where(_ => _.Type == EActionType.RetrieveExternalData).ToList(), formSchema, formName);

            if (actions.Any(_ => _.Type.Equals(EActionType.UserEmail) || _.Type.Equals(EActionType.BackOfficeEmail)))
                await _emailService.Process(actions.Where(_ => _.Type == EActionType.UserEmail || _.Type == EActionType.BackOfficeEmail).ToList());
        }
    }
}
