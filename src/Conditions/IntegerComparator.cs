using form_builder.Models;

namespace form_builder.Conditions
{
    public static class IntegerComparator
    {
        public static bool IsGreaterThan(Condition condition, Dictionary<string, dynamic> viewModel) =>
            viewModel.ContainsKey(condition.QuestionId) &&
                   int.Parse(viewModel[condition.QuestionId]) > GetIntValue(condition.ComparisonValue);

        public static bool IsGreaterThanEqualTo(Condition condition, Dictionary<string, dynamic> viewModel) =>
            viewModel.ContainsKey(condition.QuestionId) &&
                   int.Parse(viewModel[condition.QuestionId]) >= GetIntValue(condition.ComparisonValue);

        public static bool IsLessThan(Condition condition, Dictionary<string, dynamic> viewModel) =>
            viewModel.ContainsKey(condition.QuestionId) &&
                   int.Parse(viewModel[condition.QuestionId]) < GetIntValue(condition.ComparisonValue);

        public static bool IsLessThanEqualTo(Condition condition, Dictionary<string, dynamic> viewModel) =>
            viewModel.ContainsKey(condition.QuestionId) &&
                   int.Parse(viewModel[condition.QuestionId]) <= GetIntValue(condition.ComparisonValue);

        public static int GetIntValue(string comparisonValue)
        {
            var success = int.TryParse(comparisonValue, out int intValue);
            return success ? intValue : 0;
        }
    }
}
