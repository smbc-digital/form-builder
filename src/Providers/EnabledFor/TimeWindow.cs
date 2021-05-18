using System;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Providers.EnabledFor
{
    public class TimeWindow : IEnabledForProvider
    {
        public EEnabledFor Type => EEnabledFor.TimeWindow;
        public bool IsAvailable(EnabledForBase enabledFor) 
            => DateTime.Now > enabledFor.Properties.Start && DateTime.Now < enabledFor.Properties.End;
    }
}