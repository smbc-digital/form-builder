using System;
using form_builder.Enum;

namespace form_builder.Models
{
    public class Condition
    {
        public string EqualTo { get; set; }
        public string CheckboxContains { get; set; }
        public string QuestionId { get; set; }

        public int? IsBefore { get; set; }

        public int? IsAfter { get; set; }

        public string ComparisonDate { get; set; }


        public EDateUnit Unit { get; set; }
    }
}