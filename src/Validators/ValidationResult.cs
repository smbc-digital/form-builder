namespace form_builder.Validators
{
    public class ValidationResult
    {
        public ValidationResult()
        {   
            IsValid = true;
            Message = string.Empty;
        }

        public bool IsValid { get; set; }

        public string Message { get; set; }
    }
}