using System.Linq;
using System.Text;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class UploadedFilesSummaryQuestionsIsSetCheck: IFormSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(FormSchema schema)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var fileSummaryElements = schema.Pages.SelectMany(_ => _.Elements)
                .Where(_ => _.Type.Equals(EElementType.UploadedFilesSummary))
                .ToList();

            if (fileSummaryElements.Any())
            {
                fileSummaryElements.ForEach((element) =>
                {
                    if (string.IsNullOrEmpty(element.Properties.Text))
                        integrityCheckResult.AddFailureMessage("Uploaded Files Summary Question Is Set, Uploaded files summary text must not be empty.");
                        
                    if (!element.Properties.FileUploadQuestionIds.Any())
                        integrityCheckResult.AddFailureMessage("Uploaded Files Summary Question Is Set, Uploaded files summary must have atleast one file questionId specified to display the list of uploaded files.");
                });
            }

            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(FormSchema schema) => await Task.Run(() => Validate(schema));
    }
}