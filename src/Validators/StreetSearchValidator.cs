﻿using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class StreetSearchValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Type.Equals(EElementType.Street) || (element.Type.Equals(EElementType.Street) && !viewModel.IsInitial()))
                return new ValidationResult { IsValid = true };

            var streetElement = (Street)element;

            if (!viewModel.ContainsKey(streetElement.StreetSearchQuestionId))
                return new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(viewModel[streetElement.StreetSearchQuestionId]) && element.Properties.Optional)
                return new ValidationResult { IsValid = true };

            var value = (string)viewModel[streetElement.StreetSearchQuestionId];
            var isValid = true;
            var message = string.Empty;

            if (!StreetConstants.STREET_REGEX.Match(value).Success)
            {
                isValid = false;
                message = ValidationConstants.STREET_INCORRECT_FORMAT;
            }

            if (value.Length < StreetConstants.STREET_MIN_LENGTH)
            {
                isValid = false;
                message = ValidationConstants.STREET_INCORRECT_LENGTH;
            }

            return new ValidationResult
            {
                IsValid = isValid,
                Message = message
            };
        }
    }
}
