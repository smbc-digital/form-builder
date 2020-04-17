using System;
using System.Collections.Generic;
using form_builder.Comparators;
using form_builder.Enum;
using form_builder.Models;


namespace form_builder.Comparators
{
    public static class DateComparator
    {
        public static bool DateIsBefore(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var dateComparison = DateTime.Today;
            if (condition.ComparisonDate.ToLower() != "today")
            {
                dateComparison = DateTime.Parse(condition.ComparisonDate);
            }

            var newComparisonDate = GetComparisonDate(dateComparison, condition.Unit, condition.IsAfter);
            var dateValue = GetDateValue(condition.QuestionId, viewModel);



            if (DateTime.Compare(dateValue, newComparisonDate) <= 0)
            {
                return true;
            }

            return false;
        }

        public static bool DateIsAfter(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var dateComparison = DateTime.Today;
            if (condition.ComparisonDate.ToLower() != "today")
            {
                dateComparison = DateTime.Parse(condition.ComparisonDate);
            }

            var newComparisonDate = GetComparisonDate(dateComparison, condition.Unit, condition.IsAfter);

            var dateValue = GetDateValue(condition.QuestionId, viewModel);



            if (DateTime.Compare(dateValue, newComparisonDate) > 0)
            {
                return true;
            }

            return false;
        }

        private static DateTime GetComparisonDate(DateTime dateComparison, EDateUnit unit, int? isAfter)
        {
            int isAfterInt = isAfter ?? 0;

            DateTime newComparisonDate;
            switch (unit)
            {
                case (EDateUnit.Year):
                    newComparisonDate = dateComparison.AddYears(isAfterInt);
                    break;
                case (EDateUnit.Month):
                    newComparisonDate = dateComparison.AddMonths(isAfterInt);
                    break;
                case (EDateUnit.Day):
                    newComparisonDate = dateComparison.AddDays(isAfterInt);
                    break;
                default:
                    throw new Exception("No unit specifed");

            }

            return newComparisonDate;
        }

        private static DateTime GetDateValue(string questionId, Dictionary<string, dynamic> viewModel)
        {
            // for calendar object
            if(viewModel.ContainsKey(questionId))
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
