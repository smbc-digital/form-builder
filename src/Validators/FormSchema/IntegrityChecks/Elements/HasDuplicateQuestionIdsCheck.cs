using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class HasDuplicateQuestionIdsCheck: IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            var integrityCheckResult = new IntegrityCheckResult();
            var questionIds = new List<string>();

            if (element.Type != EElementType.H1
                && element.Type != EElementType.H2
                && element.Type != EElementType.H3
                && element.Type != EElementType.H4
                && element.Type != EElementType.H5
                && element.Type != EElementType.H6
                && element.Type != EElementType.Img
                && element.Type != EElementType.InlineAlert
                && element.Type != EElementType.P
                && element.Type != EElementType.Span
                && element.Type != EElementType.UL
                && element.Type != EElementType.OL
                && element.Type != EElementType.Button
                && element.Type != EElementType.HR
                && element.Type != EElementType.UploadedFilesSummary
                && element.Type != EElementType.Warning
                && element.Type != EElementType.Link
                )
            {
                questionIds.Add(element.Properties.QuestionId);
            }
            
            var hashSet = new HashSet<string>();
            if (questionIds.Any(id => !hashSet.Add(id)))
                integrityCheckResult.AddFailureMessage($"The provided json has duplicate QuestionIDs");
            
            return integrityCheckResult;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}