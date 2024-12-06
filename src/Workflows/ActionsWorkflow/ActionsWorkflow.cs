using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Models;
using form_builder.Models.Actions;
using form_builder.Services.EmailService;
using form_builder.Services.RetrieveExternalDataService;
using form_builder.Services.TemplatedEmailService;
using form_builder.Services.ValidateService;

namespace form_builder.Workflows.ActionsWorkflow
{
    public class ActionsWorkflow : IActionsWorkflow
    {
        private readonly IRetrieveExternalDataService _retrieveExternalDataService;
        private readonly IEmailService _emailService;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IValidateService _validateService;
        private readonly ITemplatedEmailService _templatedEmailService;

        public ActionsWorkflow(IRetrieveExternalDataService retrieveExternalDataService,
            IEmailService emailService,
            ISchemaFactory schemaFactory,
            IValidateService validateService,
            ITemplatedEmailService templatedEmailService)
        {
            _retrieveExternalDataService = retrieveExternalDataService;
            _emailService = emailService;
            _schemaFactory = schemaFactory;
            _validateService = validateService;
            _templatedEmailService = templatedEmailService;
        }

        public async Task Process(List<IAction> actions, FormSchema formSchema, string formName)
        {
            if (formSchema is null)
                formSchema = await _schemaFactory.Build(formName);

            if (actions.Any(_ => _.Type.Equals(EActionType.RetrieveExternalData)))
                await _retrieveExternalDataService.Process(actions.Where(_ => _.Type.Equals(EActionType.RetrieveExternalData)).ToList(), formSchema, formName);

            if (actions.Any(_ => _.Type.Equals(EActionType.UserEmail) || _.Type.Equals(EActionType.BackOfficeEmail)))
                await _emailService.Process(actions.Where(_ => _.Type.Equals(EActionType.UserEmail) || _.Type.Equals(EActionType.BackOfficeEmail)).ToList(), formName);

            if (actions.Any(_ => _.Type.Equals(EActionType.Validate)))
                await _validateService.Process(actions.Where(_ => _.Type.Equals(EActionType.Validate)).ToList(), formSchema, formName);

            if (actions.Any(_ => _.Type.Equals(EActionType.TemplatedEmail)))
                _ = _templatedEmailService.ProcessTemplatedEmail(actions.Where(_ => _.Type.Equals(EActionType.TemplatedEmail)).ToList(), formName);
        }
    }
}