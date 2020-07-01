using form_builder.Enum;
using form_builder.Models.Properties;

namespace form_builder.Models
{
    public class PageAction
    {
        public EPageActionType Type { get; set; }

        public BaseProperty Properties { get; set; }
    }
}