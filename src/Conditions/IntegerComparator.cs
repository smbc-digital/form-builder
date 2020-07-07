using System;
using System.Collections.Generic;
using System.Dynamic;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class IntegerComparator
    {
        public static bool IsGreaterThan(Condition condition, Dictionary<string, dynamic> viewModel)
        {
           if (viewModel.ContainsKey(condition.QuestionId) && 
                int.Parse(viewModel[condition.QuestionId]) > GetIntValue(condition.ComparisonValue))
                return true;
            return false;
        }

        public static bool IsGreaterThanEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            if (viewModel.ContainsKey(condition.QuestionId) &&
                 int.Parse(viewModel[condition.QuestionId]) >= GetIntValue(condition.ComparisonValue))
                return true;
            return false;
        }

        public static bool IsLessThan(Condition condition, Dictionary<string, dynamic> viewModel)
        {
          if (viewModel.ContainsKey(condition.QuestionId) && 
           int.Parse(viewModel[condition.QuestionId]) < GetIntValue(condition.ComparisonValue))
                return true;
            return false;
        }

        public static bool IsLessThanEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            if (viewModel.ContainsKey(condition.QuestionId) &&
             int.Parse(viewModel[condition.QuestionId]) <= GetIntValue(condition.ComparisonValue))
                return true;
            return false;
        }

        public static int GetIntValue(string comparisonValue)
        {
            var success = int.TryParse(comparisonValue, out int intValue);
            if (success)
                return intValue;
            return 0;
        }
    }
}
