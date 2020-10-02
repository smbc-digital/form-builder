using System.Collections.Generic;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string DocumentUploadUrl { get; set; }

        public List<SubmitSlug> SubmitSlugs { get; set; }
    }
}
