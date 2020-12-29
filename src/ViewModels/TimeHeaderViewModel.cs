using System;

namespace form_builder.ViewModels
{
    public class TimeHeaderViewModel
    {
        public string DataAriaControlsIdForMorning { get; set; }
        public string DataAriaControlsIdForEvening { get; set; }
        public DateTime Date { get; set; }
        public ETimePeriod SelectedTimePeriod { get; set; }
    }
}