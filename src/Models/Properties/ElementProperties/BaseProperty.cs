using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public string Text { get; set; }

        public string AppendText { get; set; }

        public string QuestionId { get; set; }

        public virtual string Label { get; set; }

        public virtual bool StrongLabel { get; set; } = true;

        public bool Optional { get; set; } = false;

        public bool IsDynamicallyGeneratedElement { get; set; } = false;

        public bool Email { get; set; } = false;

        public bool Postcode { get; set; } = false;

        public bool Numeric { get; set; } = false;

        public List<Option> Options { get; set; } = new List<Option>();

        public int MaxLength { get; set; } = 200;

        public int? MinLength { get; set; }

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
        public string DecimalValidationMessage { get; set; } = string.Empty;
        public string DecimalPlacesValidationMessage { get; set; } = string.Empty;
        public string IAG { get; set; }

        public bool HideOptionalText { get; set; }

        public bool isConditionalElement { get; set; } = false;

        public bool OrderOptionsAlphabetically { get; set; } = false;

        public string SummaryLabel { get; set; } = string.Empty;

        public string Warning { get; set; } = string.Empty;

        public bool Autofocus { get; set; }

        public bool SetAutofocus { get; set; }
    }
}