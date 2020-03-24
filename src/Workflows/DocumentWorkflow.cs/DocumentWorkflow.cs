namespace form_builder.Services.DocumentService
{
    public interface IDocumentWorkflow
    {
        void GenerateSummaryDocument();
    }

    public class DocumentWorkflow : IDocumentWorkflow
    {
        private IDocumentSummaryService _textfileDocumentService;

        public DocumentWorkflow(IDocumentSummaryService textfileDocumentService)
        {
            _textfileDocumentService = textfileDocumentService;
        }

        public void GenerateSummaryDocument()
        {
        }
    }
}
