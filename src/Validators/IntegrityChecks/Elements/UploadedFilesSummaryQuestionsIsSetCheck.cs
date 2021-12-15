using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class UploadedFilesSummaryQuestionsIsSetCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();

            if (!element.Type.Equals(EElementType.UploadedFilesSummary))
                return result;

            if (string.IsNullOrEmpty(element.Properties.Text))
                result.AddFailureMessage("Uploaded Files Summary Question Is Set, Uploaded files summary text must not be empty.");

            if (element.Properties.FileUploadQuestionIds.Count.Equals(0))
                result.AddFailureMessage("Uploaded Files Summary Question Is Set, Uploaded files summary must have atleast one file questionId specified to display the list of uploaded files.");

            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}