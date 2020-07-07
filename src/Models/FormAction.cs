using form_builder.Enum;
using form_builder.Models.Properties.ActionProperties;

namespace form_builder.Models
{
    public class FormAction
    {
        public EFormActionType Type { get; set; }

        public BaseActionProperty Properties { get; set; }
    }
}
