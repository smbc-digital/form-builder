using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class BookingQuestionIdExistsForCustomerAddressCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<IElement> bookingElements = schema.Pages
                .SelectMany(page => page.Elements)
                .Where(element => element.Type.Equals(EElementType.Booking))
                .ToList();

            if (bookingElements.Count.Equals(0))
                return result;

            foreach (var bookingElement in bookingElements)
            {
                if (!string.IsNullOrEmpty(bookingElement.Properties.CustomerAddressId))
                {
                    var matchingElement = schema.Pages
                        .SelectMany(page => page.Elements)
                        .FirstOrDefault(element =>
                            element.Properties.QuestionId is not null &&
                            element.Properties.QuestionId.Contains(bookingElement.Properties.CustomerAddressId));

                    if (matchingElement is null)
                        result.AddFailureMessage($"The provided json does not contain an element with questionId of {bookingElement.Properties.CustomerAddressId}' for booking element {bookingElement.Properties.QuestionId}'");
                }
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
