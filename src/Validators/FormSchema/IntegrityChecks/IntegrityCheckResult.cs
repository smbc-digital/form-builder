using System.Collections.Generic;

namespace form_builder.Validators.IntegrityChecks
{
    public class IntegrityCheckResult
    {
        public  IntegrityCheckResult()
        {
            IsValid = true;
            Messages = new List<string>();
        }

        public IntegrityCheckResult(bool isValid, ICollection<string> messages) : this()
        {
            IsValid = isValid;
            Messages = messages;
        }

        public bool IsValid { get; private set; }
        public ICollection<string> Messages { get; private set; }        

        public void AddFailureMessage (string message)
        {
            IsValid = false;
            Messages.Add($"FAILURE - {message}");
        }

        public void AddWarningMessage(string message)
        {
            Messages.Add($"WARNIING - {message}");
        }

        public static IntegrityCheckResult ValidResult => new IntegrityCheckResult { IsValid = true };
    }
}