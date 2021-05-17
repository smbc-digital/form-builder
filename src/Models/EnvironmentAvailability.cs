using System;
using System.Collections.Generic;

namespace form_builder.Models
{
    public class EnvironmentAvailability
    {
        public EnvironmentAvailability()
        {
            IsAvailable = true;
        }

        public string Environment { get; set; }

        public bool IsAvailable { get; set; }

        public List<EnabledForBase> EnabledFor { get; set; }
    }
}