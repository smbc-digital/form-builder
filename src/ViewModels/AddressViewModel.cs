using form_builder.Models.Elements;

namespace form_builder.ViewModels
{
    public class AddressViewModel
    {
        public Address Element { get; set; }

        public string ReturnURL { get; set; }
    }
}
