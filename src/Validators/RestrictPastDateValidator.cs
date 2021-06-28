﻿using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class RestrictPastDateValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (!element.Properties.RestrictPastDate || element.Type.Equals(EElementType.DatePicker))
                return new ValidationResult { IsValid = true };

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
                return new ValidationResult { IsValid = true };

            var isValidDate = DateTime.TryParse($"{valueDay}/{valueMonth}/{valueYear}", out _);

            if (!isValidDate)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.CustomValidationMessage)
                        ? element.Properties.CustomValidationMessage
                        : "Check the date and try again"
                };
            }

            var date = DateTime.Today;
            var dateOutput = DateTime.Parse($"{valueDay}/{valueMonth}/{valueYear}");

            if (dateOutput < date)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = !string.IsNullOrEmpty(element.Properties.ValidationMessageRestrictPastDate)
                        ? element.Properties.ValidationMessageRestrictPastDate
                        : "Check the date and try again"
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