namespace form_builder.Services.ValidateService;

public interface IValidateService
{
    Task Process(List<IAction> actions, FormSchema formSchema, string formName);
}