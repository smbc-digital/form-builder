using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models.Elements;

namespace form_builder.Validators
{
    public class BookingValidator : IElementValidator
    {
        public ValidationResult Validate(Element element, Dictionary<string, dynamic> viewModel)
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

            //Validate Date
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
                    IsValid = false
                };
            }

            var date = viewModel[bookingElement.TimeQuestionId];
            var isValidDate = DateTime.TryParse(date, out DateTime dateValue);

            if(!isValidDate){
                return new ValidationResult
                {
                    IsValid = false,
                    Message = string.IsNullOrEmpty(bookingElement.Properties.CustomValidationMessage) ?  "You must select a date" : bookingElement.Properties.CustomValidationMessage
                };
            }

            //Validate Time

            return new ValidationResult
            {
                IsValid = true
            };
        }
    }
}