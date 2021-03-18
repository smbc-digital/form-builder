using System;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using form_builder.Providers.Lookup;
using form_builder.Extensions;

namespace form_builder.Validators.IntegrityChecks
{
    public class DynamicLookupCheck: IFormSchemaIntegrityCheck
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
            var integrityCheckResult = new IntegrityCheckResult();

            var elements = schema.Pages
                .SelectMany(page => page.Elements)
                .Where(element => !string.IsNullOrEmpty(element.Lookup) && 
                       element.Lookup.Equals(LookUpConstants.Dynamic))
                .ToList();

            if (elements.Any())
            {
                foreach (var element in elements)
                {
                    if (element.Properties.LookupSources != null)
                    {
                        if (!element.Properties.LookupSources
                            .Any(lookup => lookup.EnvironmentName
                            .Equals(_environment.EnvironmentName, StringComparison.OrdinalIgnoreCase)))
                                integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' has no Environment details for this:({_environment.EnvironmentName}) Environment");

                        foreach (var env in element.Properties.LookupSources)
                        {
                            if (string.IsNullOrEmpty(env.EnvironmentName))
                                integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' has no Environment Name");

                            if (string.IsNullOrEmpty(env.Provider))
                                integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' has no Provider Name");

                            try
                            {
                                _lookupProviders.Get(env.Provider);
                            }
                            catch (Exception e)
                            {
                                integrityCheckResult.AddFailureMessage($"No specified Providers in form {schema.FormName}. Error Message {e.Message}");
                            }

                            if (string.IsNullOrEmpty(env.URL))
                                integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' has no API URL to submit to");

                            if (string.IsNullOrEmpty(env.AuthToken))
                                integrityCheckResult.AddFailureMessage($"The provided json '{schema.FormName}' has no auth token for the API");

                           if (!_environment.IsEnvironment("local") &&
                                !env.EnvironmentName.Equals("local", StringComparison.OrdinalIgnoreCase) &&
                                !env.URL.StartsWith("https://"))
                                integrityCheckResult.AddFailureMessage($"SubmitUrl must start with https, in form {schema.FormName}");
                        }
                    }
                    else
                    {
                        integrityCheckResult.AddFailureMessage($"Any Condition Type Check, any condition type requires a comparison value in form {schema.FormName}");
                    }
                }
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}