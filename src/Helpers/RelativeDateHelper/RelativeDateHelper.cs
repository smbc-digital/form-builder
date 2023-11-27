using System.Text.RegularExpressions;
using form_builder.Constants;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Helpers.RelativeDateHelper
{
    public class RelativeDateHelper : IRelativeDateHelper
    {
        private static dynamic[] GetDateValues(Element element, Dictionary<string, dynamic> viewModel)
        {
            var dateResult = new dynamic[3];
            dateResult[0] = viewModel.ContainsKey($"{element.Properties.QuestionId}-day")
                ? viewModel[$"{element.Properties.QuestionId}-day"]
                : null;

            dateResult[1] = viewModel.ContainsKey($"{element.Properties.QuestionId}-month")
                ? viewModel[$"{element.Properties.QuestionId}-month"]
                : null;

            dateResult[2] = viewModel.ContainsKey($"{element.Properties.QuestionId}-year")
                ? viewModel[$"{element.Properties.QuestionId}-year"]
                : null;

            return dateResult;
        }

        public bool HasValidDate(Element element, Dictionary<string, dynamic> viewModel)
        {
            var dateStrings = GetDateValues(element, viewModel);

            var isValidDate = DateTime.TryParse($"{dateStrings[0]}/{dateStrings[1]}/{dateStrings[2]}", out DateTime chosenDate);
            return dateStrings[0] is not null && dateStrings[1] is not null && dateStrings[2] is not null && isValidDate;
        }

        public DateTime ChosenDate(Element element, Dictionary<string, dynamic> viewModel)
        {
            var dateStrings = GetDateValues(element, viewModel);

            DateTime.TryParse($"{dateStrings[0]}/{dateStrings[1]}/{dateStrings[2]}", out DateTime chosenDate);
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