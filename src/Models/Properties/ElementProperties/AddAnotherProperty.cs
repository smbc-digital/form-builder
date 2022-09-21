using form_builder.Models.Elements;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public List<IElement> Elements { get; set; }

        public int MaximumFieldsets { get; set; } = 10;
    }
}
