using System.Text;
using form_builder.Configuration;
using Microsoft.Extensions.Options;

namespace form_builder.Providers.ReferenceNumbers
{
    /// <summary>
    /// This does not handle and any potential clashes - please ensure that these are checked and handled elsewhere
    /// </summary>
    public class ReferenceNumberProvider : IReferenceNumberProvider
    {
        private char[] _validReferenceCharacters;

        public ReferenceNumberProvider(IOptions<FormConfiguration> formConfig) => _validReferenceCharacters = formConfig.Value.ValidReferenceCharacters.ToCharArray();

        public string GetReference(string prefix, int length = 8) => $"{prefix}{GetReference(length)}";

        /// <summary>
        /// Create a reference of the specified length
        /// </summary>
        /// <param name="length">Required length of reference number</param>
        /// <param name="caseSensitive">
        /// Determies whether the ID will contain case sensitive characters. 
        /// Setting this value to false will result in an reference only containing upper case aplhanumeric characters, this reduces the possible number of reference combinations
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
}