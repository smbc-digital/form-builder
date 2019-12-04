using form_builder.Enum;

namespace form_builder.Models.Elements
{
    public class Span : Element, IElement
    {
        public Span()
        {
            Type = EElementType.Span;
        }
    }
}
