namespace form_builder.Validators;

public class ValidationResult(bool isValid, string message = "")
{
    public ValidationResult() : this(true, string.Empty)
    {
    }

    public bool IsValid { get; set; } = isValid;

    public string Message { get; set; } = message;
}