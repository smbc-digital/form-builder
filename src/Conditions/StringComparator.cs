using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class StringComparator
    {

        public static bool IsEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            if (viewModel.ContainsKey(condition.QuestionId) && 
                (string)viewModel[condition.QuestionId].ToLower() == condition.EqualTo.ToLower())
                return true;
            return false;
        }

        public static bool IsNullOrEmpty(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            if (viewModel.ContainsKey(condition.QuestionId) && string.IsNullOrEmpty((string)viewModel[condition.QuestionId]) == condition.IsNullOrEmpty)
                return true;
            return false;
        }

        public static bool CheckboxContains(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            if (viewModel.ContainsKey(condition.QuestionId) && viewModel[condition.QuestionId].Contains(condition.CheckboxContains))
                return true;            
            return false;
        }

    }
}
