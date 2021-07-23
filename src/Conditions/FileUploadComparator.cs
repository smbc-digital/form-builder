using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class FileUploadComparator
    {
        public static bool IsFileUploadNullOrEmpty(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var questionId = $"{condition.QuestionId}{FileUploadConstants.SUFFIX}";

            return viewModel.ContainsKey(questionId) && (viewModel[questionId] is null).Equals(bool.Parse(condition.ComparisonValue));
        }
    }
}
