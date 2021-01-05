using System;
using form_builder.Enum;
using form_builder.Utils.Extesions;

namespace form_builder.ViewModels
{
    public class TimeHeaderViewModel
    {
        public string DataAriaControlsIdForMorning { get; set; }
        public string DataAriaControlsIdForEvening { get; set; }
        public DateTime Date { get; set; }
        public string DateTextForNoScript => Date.ToFullDateFormat();
        public ETimePeriod SelectedTimePeriod { get; set; }
    }
}