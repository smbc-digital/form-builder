using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Models.Elements;

namespace form_builder.Validators.IntegrityChecks.Elements
{
    public class HasDuplicateQuestionIdsCheck : IElementSchemaIntegrityCheck
    {
        public IntegrityCheckResult Validate(IElement element)
        {
            IntegrityCheckResult result = new();
            List<string> questionIds = new();

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
                && element.Type != EElementType.Link)
                questionIds.Add(element.Properties.QuestionId);

            HashSet<string> hashSet = new();
            if (questionIds.Any(id => !hashSet.Add(id)))
                result.AddFailureMessage($"The provided json has duplicate QuestionIDs");
            
            return result;
        }

        public async Task<IntegrityCheckResult> ValidateAsync(IElement element) => await Task.Run(() => Validate(element));
    }
}