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
        /// Create and ID of the specified length
        /// </summary>
        /// <param name="length">Required length of ID number</param>
        /// <param name="caseSensitive">
        /// Determies whether the ID will contain case sensitive characters. 
        /// Setting this value to false will result in an ID only containing number and Upper case characters, this reduces the possible number of ID combinations-->
        /// </param>
        /// <returns></returns>
        internal static string GetReference(int length, bool caseSensitive = true) 
        {
            var sb = new StringBuilder(length);
            var random = new Random(); 

            for (int i=0; i<length; i++) 
            {
                if(caseSensitive)
                {
                    sb.Append(_base62chars[random.Next(62)]);
                }
                else
                {
                    sb.Append(_base62chars[random.Next(36)]);
                }
            }

            return sb.ToString();
        }       
    }   
}