using form_builder.Enum;

namespace form_builder.Models
{
    public class Condition
    {
        private ECondition _condition;
        public ECondition ConditionType {
            get
            {
                if(_condition != ECondition.Undefined)
                {
                    return _condition;
                }

                //Backwards Compatability stuff
                if(IsNullOrEmpty ==true)
                {
                   return ECondition.IsNullOrEmpty;
                }
                else if(!string.IsNullOrEmpty(EqualTo))
                {
                    return ECondition.EqualTo;
                }
                else if(!string.IsNullOrEmpty(CheckboxContains))
                {
                    return ECondition.CheckboxContains;
                }
                else if(IsBefore > 0)
                {
                    return ECondition.IsBefore;
                }
                else if(IsAfter > 0)
                {
                    return ECondition.IsAfter;
                }

                return _condition;
            }
            set
            {
                _condition = value;
            }
        }
        public bool? IsNullOrEmpty { get; set; }

        public string EqualTo { get; set; }
        
        public string CheckboxContains { get; set; }
        
        public string QuestionId { get; set; }

        public int? IsBefore { get; set; }

        public int? IsAfter { get; set; }

        public string ComparisonDate { get; set; }


        public EDateUnit Unit { get; set; }
    }
}