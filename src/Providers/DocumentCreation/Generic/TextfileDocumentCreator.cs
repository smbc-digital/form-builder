using System.IO;
using form_builder.Enum;
using System.Linq;
using System.Collections.Generic;

namespace form_builder.Providers.DocumentCreation.Generic
{
    public class TextfileDocumentCreator : IDocumentCreation
    {
        public EDocumentType DocumentType => EDocumentType.Txt;

        public byte[] CreateDocument(Dictionary<string, string> fileContent)
        {
            using (var stream = new MemoryStream())
            {
                var objstreamwriter = new StreamWriter(stream);
                fileContent.ToList().ForEach((line) => {
                    objstreamwriter.WriteLine($"{line.Key} {line.Value}");
                });
                objstreamwriter.Flush();
                objstreamwriter.Close(); 
                return stream.ToArray();
            }
        }
    }
}