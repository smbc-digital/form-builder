using System.Collections.Generic;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string Text { get; set; }

        public string QuestionId { get; set; }

        public virtual string Label { get; set; }

        public virtual bool StrongLabel { get; set; } = true;

        public bool Optional { get; set; } = false;

        public bool? Email { get; set; }

        public bool? Telephone { get; set; }

        public bool Numeric { get; set; } = false;

        public List<Option> Options { get; set; } = new List<Option>();

        public int MaxLength { get; set; } = 200;

        public string Value { get; set; } = string.Empty;

        public string Hint { get; set; } = string.Empty;

        public string SelectHint { get; set; } = string.Empty;

        public string CustomValidationMessage { get; set; } = string.Empty;

        public string SelectCustomValidationMessage { get; set; } = string.Empty;

        public string ClassName { get; set; }

        public List<string> ListItems = new List<string>();

        public bool Checked { get; set; }

        public string SelectLabel { get; set; } = string.Empty;

        public string Max { get; set; } = string.Empty;

        public string Min { get; set; } = string.Empty;

        public string TargetMapping { get; set; }

        public string UpperLimitValidationMessage { get; set; } = string.Empty;

        public bool LegendAsH1 { get; set; }

        public bool LabelAsH1 { get; set; }

        public string Purpose { get; set; }

        public bool Spellcheck { get; set; } = true;

        public string NotAnIntegerValidationMessage { get; set; } = string.Empty;

        public string IAG { get; set; }

        public bool HideOptionalText { get;set; }

        public Conditional Conditional { get; set; }
    }
}