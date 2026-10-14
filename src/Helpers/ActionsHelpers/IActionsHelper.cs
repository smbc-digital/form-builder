namespace form_builder.Helpers.ActionsHelpers;

public interface IActionHelper
{
    RequestEntity GenerateUrl(string baseUrl, FormAnswers formAnswers);
    string GetEmailToAddresses(IAction action, FormAnswers formAnswers);
    string GetEmailContent(IAction action, FormAnswers formAnswers);
}