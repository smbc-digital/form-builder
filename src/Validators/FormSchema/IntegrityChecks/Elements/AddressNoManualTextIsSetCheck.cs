using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AddressNoManualTextIsSetCheck: IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var addressElements = schema.Pages.Where(_ => _.Elements != null)
                .SelectMany(_ => _.Elements)
                .Where(_ => _.Type == EElementType.Address)
                .Where(_ => _.Properties.DisableManualAddress)
                .ToList();

            addressElements.ForEach(element =>
            {
                if (string.IsNullOrWhiteSpace(element.Properties.NoManualAddressDetailText))
                {
                    integrityCheckResult.AddFailureMessage($"Address No Manual Text Is Set Check, 'DisableManualAddress' set to 'true', 'NoManualAddressDetailText' must have value in form {schema.FormName}");
                }
            });

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}