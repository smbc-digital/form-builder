using System.Collections.Generic;
using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation.Smbc
{
    public class SmbcTextfileDocumentCreator : IDocumentCreation
    {
        public EDocumentType DocumentType => EDocumentType.Txt;

        public byte[] CreateDocument(Dictionary<string, string> fileContent)
        {
            throw new System.NotImplementedException();
        }
    }
}