using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class MinMaxValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, string> viewModel)
        {

            if ((string.IsNullOrEmpty(element.Properties.Max) && string.IsNullOrEmpty(element.Properties.Min) && element.Type != Enum.EElementType.DateInput) || !viewModel.ContainsKey(element.Properties.QuestionId))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var value = viewModel[element.Properties.QuestionId];
            var isValid = int.TryParse(value, out int output);

            if (!string.IsNullOrEmpty(element.Properties.Max) && !string.IsNullOrEmpty(element.Properties.Min))
            {
                var max = int.Parse(element.Properties.Max);
                var min = int.Parse(element.Properties.Min);
               

                if (output > max || output < min)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"{element.Properties.Label} must be between {min} and {max} inclusive"
                    };
                }

            }

            if (element.Type == Enum.EElementType.DateInput && element.Properties.QuestionId.EndsWith("-year") && string.IsNullOrEmpty(element.Properties.Max))
            {
                var maxYear = DateTime.Now.Year + 100;

                if (output > maxYear)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"Year must be less than or equal to {maxYear}"
                    };
                }
            }

            if (!string.IsNullOrEmpty(element.Properties.Max))
            {
                var max = int.Parse(element.Properties.Max);

                if (output > max)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"{element.Properties.Label} must be less than or equal to {max}"
                    };
                }

            }

            if (!string.IsNullOrEmpty(element.Properties.Min))
            {
                var min = int.Parse(element.Properties.Min);

                if (output < min)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = $"{element.Properties.Label} must be greater than or equal to {min}"
                    };
                }

            }

            return new ValidationResult
            {
                IsValid = true
            };
        }
    }
}
