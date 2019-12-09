using System.Collections.Generic;

namespace form_builder.Models.Properties
{
    public class CheckboxProperties : BaseProperty
    {
        public bool Checked { get; set; }
        public List<Option> Options { get; set; }
    }
}
