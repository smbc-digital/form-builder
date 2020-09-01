using System.Collections.Generic;
using System.Linq;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class StringComparator
    {
        public static bool IsEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
         
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue.ToLower() : condition.EqualTo.ToLower();

            return viewModel.ContainsKey(condition.QuestionId) &&
                   (string)viewModel[condition.QuestionId].ToLower() == val;
        }

        public static bool IsNullOrEmpty(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? bool.Parse(condition.ComparisonValue) : condition.IsNullOrEmpty;

            return viewModel.ContainsKey(condition.QuestionId) && string.IsNullOrEmpty((string)viewModel[condition.QuestionId]) == val;
        }

        public static bool IsOneOf(Condition condition, Dictionary<string, dynamic> viewModel) 
            => viewModel.ContainsKey(condition.QuestionId) && condition.ComparisonValue.Contains((string)viewModel[condition.QuestionId].ToLower());

        public static bool Contains(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue.ToLower() : condition.CheckboxContains.ToLower();
            
            return viewModel.ContainsKey(condition.QuestionId) && viewModel[condition.QuestionId].ToLower().Contains(val);
        }

        public static bool EndsWith(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue.ToLower() : condition.CheckboxContains.ToLower();

            return viewModel.ContainsKey(condition.QuestionId) && viewModel[condition.QuestionId].ToLower().EndsWith(val);
        }
    }
}
