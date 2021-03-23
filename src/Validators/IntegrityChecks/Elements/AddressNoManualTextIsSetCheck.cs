using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AddressNoManualTextIsSetCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (element.Type.Equals(EElementType.Address) && 
                element.Properties.DisableManualAddress && 
                string.IsNullOrWhiteSpace(element.Properties.NoManualAddressDetailText))
                    result.AddFailureMessage($"Address No Manual Text Is Set Check, 'DisableManualAddress' set to 'true', 'NoManualAddressDetailText' must have value");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}