using System;
using System.Text;

namespace form_builder.Providers.ReferenceNumbers
{
    public static class RandomIdGenerator 
    {
        private static char[] _base62chars = 
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
            .ToCharArray();

        public static string GetId(int length, bool caseSensitive = true) 
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