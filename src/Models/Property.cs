using System.Collections.Generic;

namespace form_builder.Models
{
    public class Property
    {
        public string Text { get; set; }
        
        public string QuestionId { get; set; }
        
        public string Label { get; set; }

        public bool Optional { get; set; } = false;

        public bool? Numeric { get; set; }
        
        public List<Option> Options { get; set; }
        
        public string ButtonId { get; set; }
        
        public string MaxLength { get; set; }

        public string Value { get; set; } = string.Empty;

        public string Hint { get; set; } = string.Empty;

        public string CustomValidationMessage { get; set; } = string.Empty;

        public string ValidationMessageRestrictFutureDate { get; set; } = string.Empty;

        public string ValidationMessageRestrictPastDate { get; set; } = string.Empty;

        public string ValidationMessageRestrictCurrentDate { get; set; } = string.Empty;

        public string ValidationMessageInvalidDate { get; set; } = string.Empty;

        public string ClassName { get; set; }

        public List<string> ListItems = new List<string>();

        public string Source { get; set; }

        public string AltText { get; set; }

        public bool Checked { get; set; }

        public string Day { get; set; }

        public string Month { get; set; }

        public string Year { get; set; }

        public bool RestrictFutureDate { get; set; } = false;

        public bool RestrictPastDate { get; set; } = false;

        public bool RestrictCurrentDate { get; set; } = false;
    }
}