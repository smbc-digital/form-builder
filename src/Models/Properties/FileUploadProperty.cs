using form_builder.Constants;
using System.Collections.Generic;

namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public List<string> AllowedFileTypes { get; set; }
        public int MaxFileSize { get; set; }
    }
}
