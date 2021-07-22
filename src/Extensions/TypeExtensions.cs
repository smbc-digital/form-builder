using System;
using System.Text.RegularExpressions;

namespace form_builder.Extensions
{
    public static class TypeExtensions
    {
        public static string ConvertTypeToFormattedString(this Type type) 
            => Regex.Replace(type.ToString().Replace("System", ""), @"[^A-Za-z\[\]]", "");
    }
}
