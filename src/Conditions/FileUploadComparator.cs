using Condition = form_builder.Models.Condition;

namespace form_builder.Conditions;

public static class FileUploadComparator
{
    public static bool IsFileUploadNullOrEmpty(Condition condition, Dictionary<string, dynamic> viewModel)
    {
        var questionId = $"{condition.QuestionId}{FileUploadConstants.SUFFIX}";

        return viewModel.ContainsKey(questionId) && (viewModel[questionId] is null).Equals(bool.Parse(condition.ComparisonValue));
    }
}