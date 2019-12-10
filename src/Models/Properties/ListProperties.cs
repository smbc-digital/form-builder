using System.Collections.Generic;

namespace form_builder.Models.Properties
{
    public class ListProperties : BaseProperty
    {
        public string ClassName { get; set; }

        public List<string> ListItems = new List<string>();
    }
}
