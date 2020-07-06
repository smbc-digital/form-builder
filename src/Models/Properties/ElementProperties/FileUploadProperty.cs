using System.Collections.Generic;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public List<string> AllowedFileTypes { get; set; }
        public int MaxFileSize { get; set; }
    }
}
