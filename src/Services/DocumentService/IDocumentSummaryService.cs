using form_builder.Services.DocumentService.Entities;
using System.Threading.Tasks;

namespace form_builder.Services.DocumentService
{
    public interface IDocumentSummaryService
    {
        Task<byte[]> GenerateDocument(DocumentSummaryEntity entity);
    }
}
