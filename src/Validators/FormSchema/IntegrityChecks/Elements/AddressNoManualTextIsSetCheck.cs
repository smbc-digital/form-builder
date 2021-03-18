using form_builder.Enum;
using form_builder.Models.Elements;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AddressNoManualTextIsSetCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult integrityCheckResult = new();

            if (element.Type.Equals(EElementType.Address) &&
                element.Properties.DisableManualAddress)
            {
                if (string.IsNullOrWhiteSpace(element.Properties.NoManualAddressDetailText))
                {
                    integrityCheckResult.AddFailureMessage($"Address No Manual Text Is Set Check, 'DisableManualAddress' set to 'true', 'NoManualAddressDetailText' must have value in form {schema.FormName}");
                }
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}