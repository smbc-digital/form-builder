﻿using form_builder.Enum;
using form_builder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Validators.IntegrityChecks.Form
{
    public class EnabledForTimeWindowCheck : IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            IntegrityCheckResult result = new();

            if (schema.EnvironmentAvailabilities.Where(_ => _.EnabledFor is not null).SelectMany(_ => _.EnabledFor).Any(_ => _.Type.Equals(EEnabledFor.Unknown)))
                result.AddFailureMessage("EnabledFor Check, Unknown EnabledFor type configured.");

            var TimeWindows = schema.EnvironmentAvailabilities.Where(_ => _.EnabledFor is not null)
                .SelectMany(_ => _.EnabledFor)
                .Where(_ => _.Type.Equals(EEnabledFor.TimeWindow));

            if (TimeWindows.Any())
            {               
                foreach (var timeWindow in TimeWindows)
                {
                    if (timeWindow.Properties is null)
                    {
                        result.AddFailureMessage("EnabledFor Check, EnabledFor Properties must be defined");
                    }
                    else {
                        if (timeWindow.Properties.Start.Equals(DateTime.MinValue) && timeWindow.Properties.End.Equals(DateTime.MaxValue))
                            result.AddFailureMessage("EnabledFor Check, Start and End cannot be Min and Max Value.");
                    }                               
                }     
            }
            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}
