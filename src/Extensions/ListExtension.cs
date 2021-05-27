using System.Collections.Generic;
using System.Linq;
using form_builder.Models;

namespace form_builder.Extensions
{
    public static class ListExtension
    {
        public static string ToReadableFileType(this List<string> value, string joiner)
        {
            value = value.Select(_ => _.Replace(".", string.Empty)).ToList();
            return value.Count > 1
                  ? string.Join(", ", value.Take(value.Count - 1)).ToUpper() + $" {joiner} {value.Last().ToUpper()}"
                  : value.First().ToUpper();
        }

        public static AppointmentType GetAppointmentTypeForEnvironment(this List<AppointmentType> value, string environment) => value.First(_ => _.Environment.ToLower().Equals(environment.ToLower()));
    }
}
