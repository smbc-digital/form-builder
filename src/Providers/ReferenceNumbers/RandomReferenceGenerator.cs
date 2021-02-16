using System;
using System.Text;

namespace form_builder.Providers.ReferenceNumbers
{
    public static class RandomReferenceGenerator 
    {
        private static char[] _base62chars = 
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
            .ToCharArray();
        
        /// <summary>
        /// Create a reference of the specified length
        /// </summary>
        /// <param name="length">Required length of reference number</param>
        /// <param name="caseSensitive">
        /// Determies whether the ID will contain case sensitive characters. 
        /// Setting this value to false will result in an reference only containing upper case aplhanumeric characters, this reduces the possible number of reference combinations
        /// </param>
        /// <returns></returns>
        internal static string GetReference(int length, bool caseSensitive = true) 
        {
            var sb = new StringBuilder(length);
            var random = new Random();

            for (int i=0; i<length; i++) 
            {
                sb.Append(_base62chars[random.Next(caseSensitive ? 62 : 36)]);
            }

            return sb.ToString();
        }       
    }   
}