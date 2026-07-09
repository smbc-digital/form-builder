namespace form_builder.Providers.ReferenceNumbers;

/// <summary>
/// This does not handle and any potential clashes - please ensure that these are checked and handled elsewhere
/// </summary>
public class ReferenceNumberProvider(IOptions<FormConfiguration> formConfig) : IReferenceNumberProvider
{
    private char[] _validReferenceCharacters = formConfig.Value.ValidReferenceCharacters.ToCharArray();

    public string GetReference(string prefix, int length = 8) => $"{prefix}{GetReference(length)}";

    /// <summary>
    /// Create a reference of the specified length
    /// </summary>
    /// <param name="length">Required length of reference number</param>
    /// <param name="caseSensitive">
    /// Determines whether the ID will contain case-sensitive characters. 
    /// Setting this value to false will result in a reference only containing upper case alphanumeric characters, this reduces the possible number of reference combinations
    /// </param>
    /// <returns></returns>
    private string GetReference(int length)
    {
        StringBuilder sb = new(length);
        Random random = new();
        for (int i = 0; i < length; i++)
        {
            sb.Append(_validReferenceCharacters[random.Next(_validReferenceCharacters.Length)]);
        }

        return sb.ToString();
    }
}