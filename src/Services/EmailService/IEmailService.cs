namespace form_builder.Services.EmailService;

public interface IEmailService
{
    Task Process(List<IAction> actions, string form);
}