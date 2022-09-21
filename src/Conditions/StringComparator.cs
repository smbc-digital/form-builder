using form_builder.Models;

namespace form_builder.Conditions
{
    public static class StringComparator
    {
        public static bool IsEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue : condition.EqualTo;

            return viewModel.ContainsKey(condition.QuestionId) && (bool)viewModel[condition.QuestionId].Equals(val, StringComparison.OrdinalIgnoreCase);
        }

        public static bool PaymentAmountIsEqualTo(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            return condition.QuestionId.Equals(condition.ComparisonValue);
        }

        public static bool IsNullOrEmpty(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            bool val = !string.IsNullOrEmpty(condition.ComparisonValue) ? bool.Parse(condition.ComparisonValue) : (bool)condition.IsNullOrEmpty;

            return viewModel.ContainsKey(condition.QuestionId) && string.IsNullOrEmpty((string)viewModel[condition.QuestionId]).Equals(val);
        }

        public static bool Contains(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue : condition.CheckboxContains;

            return viewModel.ContainsKey(condition.QuestionId) && viewModel[condition.QuestionId].Contains(val, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWith(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? condition.ComparisonValue : condition.CheckboxContains;

            return viewModel.ContainsKey(condition.QuestionId) && viewModel[condition.QuestionId].EndsWith(val, StringComparison.OrdinalIgnoreCase);
        }
    }
}
