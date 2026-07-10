namespace form_builder.Validators.IntegrityChecks;

public class IntegrityCheckResult()
{
    public IntegrityCheckResult(bool isValid, ICollection<string> messages) : this()
    {
        IsValid = isValid;
        Messages = messages;
    }

    public bool IsValid { get; private set; } = true;
    public ICollection<string> Messages { get; private set; } = new List<string>();

    public void AddFailureMessage(string message)
    {
        IsValid = false;
        Messages.Add($"{IntegrityChecksConstants.FAILURE}{message}");
    }

    public void AddWarningMessage(string message)
    {
        Messages.Add($"{IntegrityChecksConstants.WARNING}{message}");
    }

    public static IntegrityCheckResult ValidResult => new() { IsValid = true };
}