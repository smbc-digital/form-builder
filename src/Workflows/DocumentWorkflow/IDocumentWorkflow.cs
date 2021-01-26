using System;
using System.Threading.Tasks;
using form_builder.Enum;

namespace form_builder.Workflows.DocumentWorkflow
{
    public interface IDocumentWorkflow
    {
        Task<byte[]> GenerateSummaryDocumentAsync(EDocumentType documentType, Guid id);
    }
}
