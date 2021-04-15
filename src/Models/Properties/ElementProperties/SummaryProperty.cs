using System.Collections.Generic;
using System.Linq;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public bool AllowEditing { get; set; }
        public List<Section> Sections { get; set; }
        public bool HasSummarySectionsDefined => Sections is not null && Sections.Any();
    }

    public class Section
    {
        public string Title { get; set; }
        public List<string> Pages { get; set; }
    }
}