using System;
using System.Collections.Generic;

namespace form_builder.Models
{
    public class FileUploadModel
    {
        public string Key { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public List<String> AllowedFileTypes { get; set; }
    }
}
