using System.Collections.Generic;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public interface IElementValidator
    {
        ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm);
    }
}