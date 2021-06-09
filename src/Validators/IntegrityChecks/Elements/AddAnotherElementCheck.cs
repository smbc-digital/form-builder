﻿using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class AddAnotherElementCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (!element.Type.Equals(EElementType.AddAnother))
                return result;

            if (element.Properties is null)
            {
                result.AddFailureMessage("Add Another Element Check : No Properties section found for this element.");
                return result;
            }

            if (element.Properties.Elements is null)
            {
                result.AddFailureMessage("Add Another Element Check : No Elements section found for this element.");
                return result;
            }

            var nestedElements = element.Properties.Elements;
            if (nestedElements.Count.Equals(0))
            {
                result.AddFailureMessage("Add Another Element Check : No elements found.");
                return result;
            }

            foreach (IElement nestedElement in nestedElements)
            {
                if (!AddAnotherConstants.ValidElements.Any(elementType => elementType.Equals(nestedElement.Type)))
                {
                    result.AddFailureMessage($"Add Another Element Check : Invalid - {nestedElement.Type} not valid element type for this feature.");
                }
            }

            return result;
        }
        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}
