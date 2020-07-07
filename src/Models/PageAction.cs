using form_builder.Enum;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Models.Properties.ElementProperties;

namespace form_builder.Models
{
    public class PageAction
    {
        public EPageActionType Type { get; set; }

        public BaseActionProperty Properties { get; set; }
    }
}