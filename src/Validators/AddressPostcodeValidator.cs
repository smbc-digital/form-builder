using System.Collections.Generic;
using System.Text.RegularExpressions;
using form_builder.Models.Elements;


namespace form_builder.Validators
{
    public class AddressPostcodeValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {
            if (element.Type != Enum.EElementType.Address)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (!viewModel.ContainsKey(element.Properties.QuestionId + "-postcode"))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (string.IsNullOrEmpty(viewModel[element.Properties.QuestionId + "-postcode"]) && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

           
            var value = viewModel[element.Properties.QuestionId + "-postcode"];

            var isValid = true;
            var regex = new Regex(@"^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})$");

            Match match = regex.Match(value);

            if (!match.Success)
            {
                isValid = false;
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = isValid ? string.Empty : $"{ element.Properties.Label} must be a valid postcode"
            };
        }
    }
}
