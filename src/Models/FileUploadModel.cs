using System.Collections.Generic;

namespace form_builder.Models
{
    public class FileUploadModel
    {
        public string Key { get; set; }

        public string TrustedOriginalFileName { get; set; }

        public string UntrustedOriginalFileName { get; set; }
    }
}