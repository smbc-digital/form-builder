using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using form_builder.Models;
using form_builder.Constants;
using form_builder.Extensions;
using form_builder.Models.Elements;
using form_builder.Providers.Lookup;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class DynamicLookupCheck : IFormSchemaIntegrityCheck
    {
        IWebHostEnvironment _environment;
        IEnumerable<ILookupProvider> _lookupProviders;
        public DynamicLookupCheck(IWebHostEnvironment environment, IEnumerable<ILookupProvider> lookupProviders)
        {
            _environment = environment;
            _lookupProviders = lookupProviders;
        }

        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            List<IElement> elements = schema.Pages
                .SelectMany(page => page.Elements)
                .Where(element => !string.IsNullOrEmpty(element.Lookup) &&
                       element.Lookup.Equals(LookUpConstants.Dynamic))
                .ToList();

            if (elements.Count == 0)
                return result;

            foreach (var element in elements)
            {
                if (element.Properties.LookupSources is not null)
                {
                    result.AddFailureMessage($"Any Condition Type Check, any condition type requires a comparison value in form.");
                    return result;
                }

                if (!element.Properties.LookupSources
                    .Any(lookup => lookup.EnvironmentName
                    .Equals(_environment.EnvironmentName, StringComparison.OrdinalIgnoreCase)))
                        result.AddFailureMessage($"The provided json has no Environment details for this:({_environment.EnvironmentName}) Environment");

                foreach (var env in element.Properties.LookupSources)
                {
                    if (string.IsNullOrEmpty(env.EnvironmentName))
                        result.AddFailureMessage($"The provided json has no Environment Name");

                    if (string.IsNullOrEmpty(env.Provider))
                        result.AddFailureMessage($"The provided json has no Provider Name");

                    try
                    {
                        _lookupProviders.Get(env.Provider);
                    }
                    catch (Exception e)
                    {
                        result.AddFailureMessage($"No specified Providers in form. Error Message {e.Message}");
                    }

                    if (string.IsNullOrEmpty(env.URL))
                        result.AddFailureMessage($"The provided json has no API URL to submit to");

                    if (string.IsNullOrEmpty(env.AuthToken))
                        result.AddFailureMessage($"The provided json has no auth token for the API");

                    if (!_environment.IsEnvironment("local") &&
                         !env.EnvironmentName.Equals("local", StringComparison.OrdinalIgnoreCase) &&
                         !env.URL.StartsWith("https://"))
                        result.AddFailureMessage($"SubmitUrl must start with https, in form.");
                }
            }

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
