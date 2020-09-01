using form_builder.Enum;

namespace form_builder.Models
{
    public class Condition
    {
        private ECondition _condition;
        public ECondition ConditionType
        {
            get
            {
                if (_condition != ECondition.Undefined)
                    return _condition;

                //Backwards compatibility stuff
                if (IsNullOrEmpty != null)
                    return ECondition.IsNullOrEmpty;

                if (!string.IsNullOrEmpty(EqualTo))
                    return ECondition.EqualTo;

                if (!string.IsNullOrEmpty(CheckboxContains))
                    return ECondition.Contains;

                if (IsBefore > 0)
                    return ECondition.IsBefore;

                return IsAfter > 0 ? ECondition.IsAfter : _condition;
            }

            set => _condition = value;
        }

        public bool? IsNullOrEmpty { get; set; }

        public string EqualTo { get; set; }

        public string CheckboxContains { get; set; }

        public string QuestionId { get; set; }

        public int? IsBefore { get; set; }

        public int? IsAfter { get; set; }

        public string ComparisonValue { get; set; }

        public string ComparisonDate { get; set; }

        public EDateUnit Unit { get; set; }
    }
}