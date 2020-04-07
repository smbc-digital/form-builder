﻿using System;
using System.Collections.Generic;
using System.Globalization;
using form_builder.Models.Elements;
using form_builder.Enum;

namespace form_builder.Validators
{
    public class RestrictPastDateValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
        {
            if (!element.Properties.RestrictPastDate || element.Type == EElementType.DatePicker)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var valueDay = viewModel.ContainsKey($"{element.Properties.QuestionId}-day")
                ? viewModel[$"{element.Properties.QuestionId}-day"]
                : null;

            var valueMonth = viewModel.ContainsKey($"{element.Properties.QuestionId}-month")
                ? viewModel[$"{element.Properties.QuestionId}-month"]
                : null;

            var valueYear = viewModel.ContainsKey($"{element.Properties.QuestionId}-year")
                ? viewModel[$"{element.Properties.QuestionId}-year"]
                : null;

            if (element.Properties.Optional && string.IsNullOrEmpty(valueDay) && string.IsNullOrEmpty(valueMonth) && string.IsNullOrEmpty(valueYear))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var isValidDate = DateTime.TryParseExact($"{valueDay}/{valueMonth}/{valueYear}", "dd/MM/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out DateTime dateOutput);

            if (!isValidDate)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage) ? element.Properties.CustomValidationMessage : "Check the date and try again"
                };
            }

            var date = DateTime.Today;

          
            if (dateOutput < date)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageRestrictPastDate) ? element.Properties.ValidationMessageRestrictPastDate : "Check the date and try again"
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = string.Empty
            };
        }
    }
}
