using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Services.DocumentService.Entities
{
    public class DocumentSummaryEntity
    {
        public EDocumentType DocumentType;
        public FormAnswers PreviousAnswers;
        public FormSchema FormSchema;
    }
}
