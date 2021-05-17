using System;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.EnabledFor
{
    public class TimeWindow : IEnabledFor
    {
        public EEnabledFor Type => EEnabledFor.TimeWindow;
        public bool IsAvailable(EnabledForBase enabledFor) 
            => DateTime.Now > enabledFor.Properties.Start && DateTime.Now < enabledFor.Properties.End;
    }
}