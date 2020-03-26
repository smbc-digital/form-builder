using System.Collections.Generic;
using form_builder.Enum;

namespace form_builder.Providers.DocumentCreation
{
    public interface IDocumentCreation
    {
        EDocumentType DocumentType { get; }
        
        byte[] CreateDocument(List<string> fileContent);
    }
}