using System;
using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class BookingValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel, FormSchema baseForm)
        {
            if (element.Type != EElementType.Booking ||
                (element.Type == EElementType.Booking && viewModel.IsCheckYourBooking()))
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            var bookingElement = (Booking)element;

            var containsBookingDate = viewModel.ContainsKey(bookingElement.DateQuestionId);
            var containsBookingStartTime = viewModel.ContainsKey(bookingElement.StartTimeQuestionId);

            if (!containsBookingDate && !containsBookingStartTime && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if (!containsBookingDate)
            {
                return new ValidationResult
                {
                    Message = ValidationMessage(containsBookingDate, containsBookingStartTime, element.Properties.CustomValidationMessage),
                    IsValid = false
                };
            }

            var date = viewModel[bookingElement.DateQuestionId];
            var isValidDate = DateTime.TryParse(date, out DateTime dateValue);

            if (!isValidDate)
            {
                return new ValidationResult
                {
                    Message = ValidationMessage(isValidDate, containsBookingStartTime, element.Properties.CustomValidationMessage),
                    IsValid = false
                };
            }

            return VerifyStartAndEndTime(bookingElement, viewModel, (containsBookingDate && isValidDate));
        }

        private ValidationResult VerifyStartAndEndTime(Booking element, Dictionary<string, dynamic> viewModel, bool isDateValid)
        {
            var containsBookingStartTime = viewModel.ContainsKey(element.StartTimeQuestionId);
            var containsBookingEndTime = viewModel.ContainsKey(element.EndTimeQuestionId);

            if (!containsBookingStartTime || !containsBookingEndTime)
            {
                return new ValidationResult
                {
                    Message = ValidationMessage(isDateValid, false, element.Properties.CustomValidationMessage),
                    IsValid = false
                };
            }

            var startTime = viewModel[element.StartTimeQuestionId];
            var endTime = viewModel[element.EndTimeQuestionId];
            var isValidStartTime = DateTime.TryParse(startTime, out DateTime startTimeValue);
            var isValidEndTime = DateTime.TryParse(endTime, out DateTime endTimeValue);

            if (!isValidStartTime || !isValidEndTime)
            {
                return new ValidationResult
                {
                    Message = ValidationMessage(isDateValid, false, element.Properties.CustomValidationMessage),
                    IsValid = false
                };
            }

            return new ValidationResult
            {
                IsValid = true
            };
        }

        private string ValidationMessage(bool isDateValid, bool isTimeValid, string customValidationMessage)
        {
            if (!isDateValid && !isTimeValid)
                return string.IsNullOrEmpty(customValidationMessage) ? ValidationConstants.BOOKING_DATE_AND_TIME_EMPTY : customValidationMessage;

            if (!isTimeValid)
                return string.IsNullOrEmpty(customValidationMessage) ? ValidationConstants.BOOKING_TIME_EMPTY : customValidationMessage;

            return string.IsNullOrEmpty(customValidationMessage) ? ValidationConstants.BOOKING_DATE_EMPTY : customValidationMessage;
        }
    }
}