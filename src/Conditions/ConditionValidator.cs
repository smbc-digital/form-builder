using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Conditions
{
    public class ConditionValidator
    {
        private Dictionary<ECondition, Func<Condition, Dictionary<string, dynamic>, bool>> ConditionList =

        new Dictionary<ECondition, Func<Condition, Dictionary<string, dynamic>, bool>>()
        {
            { ECondition.IsBefore, DateComparator.DateIsBefore },
            { ECondition.IsAfter, DateComparator.DateIsAfter },
            { ECondition.IsNullOrEmpty, StringComparator.IsNullOrEmpty },
            { ECondition.EqualTo, StringComparator.IsEqualTo },
            { ECondition.CheckboxContains, StringComparator.CheckboxContains },
            { ECondition.GreaterThan, IntegerComparator.IsGreaterThan },
            { ECondition.LessThan, IntegerComparator.IsLessThan },
            { ECondition.GreaterThanEqualTo, IntegerComparator.IsGreaterThanEqualTo },
            { ECondition.LessThanEqualTo, IntegerComparator.IsLessThanEqualTo },
            { ECondition.EndsWith, StringComparator.EndsWith }
        };

        public bool IsValid(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            return ConditionList[condition.ConditionType](condition, viewModel);
        }

    }
}
