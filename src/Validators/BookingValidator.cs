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

            if(!containsBookingDate && element.Properties.Optional)
            {
                return new ValidationResult
                {
                    IsValid = true
                };
            }

            if(!containsBookingDate)
            {
                return new ValidationResult
                {
                    Message = string.IsNullOrEmpty(bookingElement.Properties.CustomValidationMessage) ?  ValidationConstants.BOOKING_DATE_EMPTY : bookingElement.Properties.CustomValidationMessage,
                    IsValid = false
                };
            }

            var date = viewModel[bookingElement.DateQuestionId];
            var isValidDate = DateTime.TryParse(date, out DateTime dateValue);

            if(!isValidDate){
                return new ValidationResult
                {
                    IsValid = false,
                    Message = string.IsNullOrEmpty(bookingElement.Properties.CustomValidationMessage) ?  ValidationConstants.BOOKING_DATE_EMPTY : bookingElement.Properties.CustomValidationMessage
                };
            }

            return new ValidationResult
            {
                IsValid = true
            };
        }
    }
}