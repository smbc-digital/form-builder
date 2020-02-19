using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public List<String> AllowedFileTypes { get; set; }
        public int MaxFileSize { get; set; }
    }
}
