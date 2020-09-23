using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Extensions
{
    public static class ListExtension
    {
        public static string ToReadableFileType(this List<string> value)
        {
            return value.Count > 1
                  ? string.Join(", ", value.Take(value.Count - 1)).ToUpper() + $" or {value.Last().ToUpper()}"
                  : value.First().ToUpper();
        }
    }
}
