using form_builder.Enum;
using form_builder.Utils.Extensions;

namespace form_builder.ViewModels
{
    public class TimeHeaderViewModel
    {
        public string DataAriaControlsIdForMorning { get; set; }
        public string DataAriaControlsIdForAfternoon { get; set; }
        public DateTime Date { get; set; }
        public string DateTextForNoScript => Date.ToFullDateFormat();
        public ETimePeriod SelectedTimePeriod { get; set; }
    }
}