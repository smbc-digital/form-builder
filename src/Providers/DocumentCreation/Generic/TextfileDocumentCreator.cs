using System.IO;
using form_builder.Enum;
using System.Collections.Generic;

namespace form_builder.Providers.DocumentCreation.Generic
{
    public class TextfileDocumentCreator : IDocumentCreation
    {
        public EDocumentType DocumentType => EDocumentType.Txt;
        public byte[] CreateDocument(List<string> fileContent)
        {
            using (var stream = new MemoryStream())
            {
                var objstreamwriter = new StreamWriter(stream);
                fileContent.ForEach((line) => {
                    objstreamwriter.WriteLine(line);
                });
                objstreamwriter.Flush();
                objstreamwriter.Close(); 
                return stream.ToArray();
            }
        }
    }
}