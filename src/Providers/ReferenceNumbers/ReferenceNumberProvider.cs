namespace form_builder.Providers.ReferenceNumbers
{
    /// <summary>
    /// This does not handle and any potential clashes - please ensure that these are checked and handled elsewhere
    /// </summary>
    public class ReferenceNumberProvider : IReferenceNumberProvider
    {
        public string GetReference(string prefix, int length = 8, bool caseSensitive = true) => $"{prefix}{RandomReferenceGenerator.GetReference(length, caseSensitive)}";
    }
}