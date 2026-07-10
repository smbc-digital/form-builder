namespace form_builder.Validators;

public interface IElementValidator
{
    ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm);
}