using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Validators
{
    public interface IElementValidator
    {
        ValidationResult Validate(Element element, Dictionary<string, string> viewModel);
    }
}