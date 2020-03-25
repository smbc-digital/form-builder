using System.Collections.Generic;
using form_builder.Enum;

namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public bool DocumentDownload { get; set; }
        public List<EDocumentType> DocumentType { get; set; }
    }
}
