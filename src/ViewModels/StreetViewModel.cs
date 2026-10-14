using Street = form_builder.Models.Elements.Street;

namespace form_builder.ViewModels;

public class StreetViewModel
{
    public Street Element { get; set; }

    public string ReturnURL { get; set; }
}