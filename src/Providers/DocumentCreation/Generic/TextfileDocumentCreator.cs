using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation.Generic
{
    public class TextfileDocumentCreator : IDocumentCreation
    {
        public EDocumentType DocumentType => EDocumentType.Txt;

        public void CreateDocument()
        {
            throw new System.NotImplementedException();
        }
    }
}