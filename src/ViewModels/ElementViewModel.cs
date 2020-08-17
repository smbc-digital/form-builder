using form_builder.Models.Elements;

namespace form_builder.ViewModels
{
    public class ElementViewModel
    {
        public Element Element { get; set; }

        public string ReturnURL { get; set; }
        public string ManualAddressURL { get; set; }
    }
}
