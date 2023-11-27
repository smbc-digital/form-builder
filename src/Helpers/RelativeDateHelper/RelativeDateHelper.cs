using System.Text.RegularExpressions;
using form_builder.Constants;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Helpers.RelativeDateHelper
{
    public class RelativeDateHelper
    {
        private readonly dynamic _valueDay;
        private readonly dynamic _valueMonth;
        private readonly dynamic _valueYear;

        public RelativeDateHelper(Element element, Dictionary<string, dynamic> viewModel)
        {
            _valueDay = viewModel.ContainsKey($"{element.Properties.QuestionId}-day")
                ? viewModel[$"{element.Properties.QuestionId}-day"]
                : null;

            _valueMonth = viewModel.ContainsKey($"{element.Properties.QuestionId}-month")
                ? viewModel[$"{element.Properties.QuestionId}-month"]
                : null;

            _valueYear = viewModel.ContainsKey($"{element.Properties.QuestionId}-year")
                ? viewModel[$"{element.Properties.QuestionId}-year"]
                : null;
        }

        public bool HasValidDate()
        {
            var isValidDate = DateTime.TryParse($"{_valueDay}/{_valueMonth}/{_valueYear}", out DateTime chosenDate);
            return _valueDay is not null && _valueMonth is not null && _valueYear is not null && isValidDate;
        }

        public DateTime ChosenDate()
        {
            DateTime.TryParse($"{_valueDay}/{_valueMonth}/{_valueYear}", out DateTime chosenDate);
            return chosenDate;
        }

        public RelativeDate GetRelativeDate(string relativeDateString)
        {
            var tokens = relativeDateString.Split('-');

            return new RelativeDate()
            {
                Ammount = Convert.ToInt32(tokens[0].Trim()),
                Unit = tokens[1].Trim().ToUpper(),
                Type = tokens[2].Trim().ToUpper()
            };
        }
    }
}