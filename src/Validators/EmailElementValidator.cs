namespace form_builder.Validators;

public class EmailElementValidator : IElementValidator
{
    public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
    {
        if (!element.Properties.Email)
            return new ValidationResult { IsValid = true };

        if (string.IsNullOrEmpty(viewModel[element.Properties.QuestionId]) && element.Properties.Optional)
            return new ValidationResult { IsValid = true };

        if (!viewModel.ContainsKey(element.Properties.QuestionId))
            return new ValidationResult { IsValid = true };

        var value = viewModel[element.Properties.QuestionId];
        var isValid = true;
        var regex = new Regex(@"^(?!.*\.\.)[A-Za-z0-9._%+-]+@([A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}$");
        Match match = regex.Match(value);

        if (!match.Success) 
            isValid = false;

        return new ValidationResult
        {
            IsValid = isValid,
            Message = ValidationConstants.EMAIL_INCORRECT_FORMAT
        };
    }
}