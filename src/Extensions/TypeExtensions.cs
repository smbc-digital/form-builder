using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Extensions
{
    public static class TypeExtensions
    {
        public static string ConvertTypeToFormattedString(this Type type) => type.ToString().Replace("System.", "");
    }
}
