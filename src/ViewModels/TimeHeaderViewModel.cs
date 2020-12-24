using System;

namespace form_builder.ViewModels
{
    public class TimeHeaderViewModel
    {
        public string AmId { get; set; }
        public string PmId { get; set; }
        public DateTime Date { get; set; }
        public ETimePeriod Checked { get; set; }
    }
}