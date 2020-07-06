using System;
using System.Collections.Generic;
using System.Dynamic;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class IntegerComparator
    {
        public static bool IsMoreThan(Condition condition, Dictionary<string, dynamic> viewModel)
        {
           if (viewModel.ContainsKey(condition.QuestionId) && 
                int.Parse(viewModel[condition.QuestionId]) > GetIntValue(condition.ComparisonValue))
                return true;
            return false;
        }

        public static bool IsMoreThanEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            if (viewModel.ContainsKey(condition.QuestionId) &&
                 int.Parse(viewModel[condition.QuestionId]) >= GetIntValue(condition.comparisonValue))
                return true;
            return false;
        }

        public static bool IsFewerThan(Condition condition, Dictionary<string, dynamic> viewModel)
        {
          if (viewModel.ContainsKey(condition.QuestionId) && 
           int.Parse(viewModel[condition.QuestionId]) < GetIntValue(condition.ComparisonValue))
                return true;
            return false;
        }

        public static bool IsFewerThanEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            if (viewModel.ContainsKey(condition.QuestionId) &&
             int.Parse(viewModel[condition.QuestionId]) <= GetIntValue(condition.comparisonValue))
                return true;
            return false;
        }

        public static int GetIntValue(string comparisonValue)
        {
            var success = int.TryParse(ComparisonValue, out int intValue);
            if (success)
                return intValue;
            return 0;
        }
    }
}
