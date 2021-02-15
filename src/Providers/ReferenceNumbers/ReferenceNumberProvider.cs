using System;
using System.Text;

namespace form_builder.Providers.ReferenceNumbers
{  
    public interface IReferenceNumberProvider
    {
        string GetReference(string prefix, int length=8);
    }

    /// <summary>
    /// This does not handle and any potential clashes - please ensure that these are check and handled elsewhere
    /// </summary>
    public class ReferenceNumberProvider: IReferenceNumberProvider
    {
        public string GetReference(string prefix, int length=8)
        {
            return $"{prefix}{RandomIdGenerator.GetId(length)}";
        }
    }
    
    public static class RandomIdGenerator 
    {
        private static char[] _base62chars = 
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
            .ToCharArray();

        private static Random _random = new Random();

        public static string GetId(int length, bool caseSensitive = true) 
        {
            var sb = new StringBuilder(length);

            for (int i=0; i<length; i++) 
            {
                if(caseSensitive)
                {
                    sb.Append(_base62chars[_random.Next(62)]);
                }
                else
                {
                    sb.Append(_base62chars[_random.Next(36)]);
                }
            }

            return sb.ToString();
        }       
    }   
}