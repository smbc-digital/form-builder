using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class UploadedFilesSummaryQuestionsIsSetCheck: IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            var integrityCheckResult = new IntegrityCheckResult();

            if (!element.Type.Equals(EElementType.UploadedFilesSummary))
                return integrityCheckResult;
          
            if (string.IsNullOrEmpty(element.Properties.Text))
                integrityCheckResult.AddFailureMessage("Uploaded Files Summary Question Is Set, Uploaded files summary text must not be empty.");
                        
            if (!element.Properties.FileUploadQuestionIds.Any())
                integrityCheckResult.AddFailureMessage("Uploaded Files Summary Question Is Set, Uploaded files summary must have atleast one file questionId specified to display the list of uploaded files.");

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}