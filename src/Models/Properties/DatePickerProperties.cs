using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models.Properties
{
    public class DatePickerProperties : BaseProperty
    {
        string max { get; set; } = string.Empty;
        string min { get; set; } = string.Empty;
    }
}
