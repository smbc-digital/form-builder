using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Models;

namespace form_builder.Conditions
{
    public static class FileUploadComparator
    {
        public static bool IsFileUploadNullOrEmpty(Condition condition, Dictionary<string, dynamic> viewModel)
        {
            var val = !string.IsNullOrEmpty(condition.ComparisonValue) ? bool.Parse(condition.ComparisonValue) : condition.IsNullOrEmpty;
            var questionId = condition.QuestionId += FileUploadConstants.SUFFIX;

            return viewModel.ContainsKey(questionId) && (viewModel[questionId] == null) == val;
        }
    }
}
