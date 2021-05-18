using System;

namespace form_builder.Models.Properties.EnabledForProperties
{
    public class EnabledForProperties
    {
        public DateTime Start { get; set; } = DateTime.MinValue;
        public DateTime End { get; set; } = DateTime.MaxValue;
    }
}