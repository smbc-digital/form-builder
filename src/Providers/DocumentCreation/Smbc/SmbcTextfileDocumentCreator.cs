using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation.Smbc
{
    public class SmbcTextfileDocumentCreator : IDocumentCreation
    {
        public EDocumentType DocumentType => EDocumentType.Txt;

        public void CreateDocument()
        {
            throw new System.NotImplementedException();
        }
    }
}