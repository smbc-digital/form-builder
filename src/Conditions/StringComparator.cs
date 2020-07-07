﻿using System;
using System.Collections.Generic;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class StringComparator
    {
        public static bool IsEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
         
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue.ToLower() : condition.EqualTo.ToLower();

            if (viewModel.ContainsKey(condition.QuestionId) &&
                (string)viewModel[condition.QuestionId].ToLower() == val)
                    return true;
            return false;
        }

        public static bool IsNullOrEmpty(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? bool.Parse(condition.ComparisonValue) : condition.IsNullOrEmpty;

            if (viewModel.ContainsKey(condition.QuestionId) && string.IsNullOrEmpty((string)viewModel[condition.QuestionId]) == val)
                return true;
            return false;
        }

        public static bool CheckboxContains(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue.ToLower() : condition.CheckboxContains.ToLower();
            
            if (viewModel.ContainsKey(condition.QuestionId) && viewModel[condition.QuestionId].ToLower().Contains(val))
                    return true;
            
            return false;
        }

        public static bool EndsWith(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue.ToLower() : condition.CheckboxContains.ToLower();

            if (viewModel.ContainsKey(condition.QuestionId) && viewModel[condition.QuestionId].ToLower().EndsWith(val))
                return true;

            return false;
        }
    }
}
