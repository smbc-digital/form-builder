using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class DateComparator
    {
        public static bool DateIsBefore(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var dateComparison = DateTime.Today;
            if (!condition.ComparisonDate.Equals("today", StringComparison.OrdinalIgnoreCase))
            {
                dateComparison = DateTime.Parse(condition.ComparisonDate);
            }

            var isBefore = !string.IsNullOrEmpty(condition.ComparisonValue) ? int.Parse(condition.ComparisonValue) : condition.IsBefore.Value;
            var newComparisonDate = GetComparisonDate(dateComparison, condition.Unit, isBefore);
            var dateValue = GetDateValue(condition.QuestionId, viewModel);

            return DateTime.Compare(dateValue, newComparisonDate) < 0;
        }

        public static bool DateIsAfter(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var dateComparison = DateTime.Today;
            if (!condition.ComparisonDate.Equals("today", StringComparison.OrdinalIgnoreCase))
                dateComparison = DateTime.Parse(condition.ComparisonDate);

            var isAfter = !string.IsNullOrEmpty(condition.ComparisonValue) ? int.Parse(condition.ComparisonValue) : condition.IsAfter.Value;
            var newComparisonDate = GetComparisonDate(dateComparison, condition.Unit, isAfter);
            var dateValue = GetDateValue(condition.QuestionId, viewModel);

            return DateTime.Compare(dateValue, newComparisonDate) > 0;
        }

        public static bool DateIsEqual(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var dateComparison = DateTime.Today;
            if (!condition.ComparisonDate.Equals("today", StringComparison.OrdinalIgnoreCase))
                dateComparison = DateTime.Parse(condition.ComparisonDate);

            var isEqualTo = !string.IsNullOrEmpty(condition.ComparisonValue) ? int.Parse(condition.ComparisonValue) : condition.IsBefore.Value;
            var newComparisonDate = GetComparisonDate(dateComparison, condition.Unit, isEqualTo);
            var dateValue = GetDateValue(condition.QuestionId, viewModel);

            return DateTime.Compare(dateValue, newComparisonDate).Equals(0);
        }

        private static DateTime GetComparisonDate(DateTime dateComparison, EDateUnit unit, int isAfter)
        {
            DateTime newComparisonDate;
            switch (unit)
            {
                case (EDateUnit.Year):
                    newComparisonDate = dateComparison.AddYears(isAfter);
                    break;
                case (EDateUnit.Month):
                    newComparisonDate = dateComparison.AddMonths(isAfter);
                    break;
                case (EDateUnit.Day):
                    newComparisonDate = dateComparison.AddDays(isAfter);
                    break;
                default:
                    throw new Exception("No unit specifed");
            }

            return newComparisonDate;
        }

        private static DateTime GetDateValue(string questionId, Dictionary<string, dynamic> viewModel)
        {
            // for calendar object
            if (viewModel.ContainsKey(questionId))
            {
                return DateTime.Parse(viewModel[questionId]);
            }

            int years = int.Parse(viewModel[questionId + "-year"]);
            int months = int.Parse(viewModel[questionId + "-month"]);
            int days = int.Parse(viewModel[questionId + "-day"]);

            return new DateTime(years, months, days);
        }
    }
}
