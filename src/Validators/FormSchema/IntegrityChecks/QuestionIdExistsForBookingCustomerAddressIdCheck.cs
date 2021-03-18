using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks
{
    public class QuestionIdExistsForBookingCustomerAddressIdCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            List<IElement> bookingElements = schema.Pages
                .SelectMany(_ => _.Elements)
                .Where(_ => _.Type.Equals(EElementType.Booking))
                .ToList();

            if (bookingElements.Count > 0)
            {
                foreach (var bookingElement in bookingElements)
                {
                    if (!string.IsNullOrEmpty(bookingElement.Properties.CustomerAddressId))
                    {
                        var matchingElement = schema.Pages.SelectMany(_ => _.Elements)
                            .FirstOrDefault(_ =>
                                _.Properties.QuestionId != null &&
                                _.Properties.QuestionId.Contains(bookingElement.Properties.CustomerAddressId));

                        if (matchingElement == null)
                            integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' does not contain an element with questionId of {bookingElement.Properties.CustomerAddressId}' for booking element {bookingElement.Properties.QuestionId}'");
                    }
                }
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}