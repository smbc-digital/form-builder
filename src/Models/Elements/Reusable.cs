using form_builder.Enum;

namespace form_builder.Models.Elements
{
    public class Reusable : Element
    {
        public string ElementRef { get; set; } = string.Empty;
        public Reusable()
        {
            Type = EElementType.Reusable;
        }
    }
}
