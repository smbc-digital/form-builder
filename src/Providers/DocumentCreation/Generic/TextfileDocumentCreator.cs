using form_builder.Enum;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;


namespace form_builder.Providers.DocumentCreation.Generic
{
    public class TextfileDocumentCreator : IDocumentCreation
    {
        public EProviderPriority Priority => EProviderPriority.High;
        public EDocumentType DocumentType => EDocumentType.Txt;
        public byte[] CreateDocument(List<string> fileContent)
        {
            using (var stream = new MemoryStream())
            {
                var objStreamWriter = new StreamWriter(stream);
                fileContent.ForEach((line) =>
                {
                    objStreamWriter.WriteLine(line);
                });
                objStreamWriter.Flush();
                objStreamWriter.Close();
                return stream.ToArray();
            }
        }

        public byte[] CreateHtmlDocument(List<string> fileContent)
        {
            using (var stream = new MemoryStream())
            {
                var objStreamWriter = new StreamWriter(stream);
                objStreamWriter.WriteLine("<div>");
                objStreamWriter.WriteLine("<h2>Form answers</h2>");
                objStreamWriter.WriteLine("<hr />");
                fileContent.ForEach((line) =>
                {
                    objStreamWriter.WriteLine($"<p>{line}</p>");
                });
                objStreamWriter.WriteLine("<hr />");
                objStreamWriter.WriteLine("</div>");
                objStreamWriter.Flush();
                objStreamWriter.Close();
                return stream.ToArray();
            }
        }

        public byte[] CreatePdfDocument(List<string> fileContent)
         {
            var pdfdocument = new PdfDocument();
            var pdfPage = pdfdocument.AddPage();
            pdfPage.Size = PdfSharpCore.PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(pdfPage);
            gfx.DrawString("Form Answers", new XFont("Verdana", 18, XFontStyle.Bold), XBrushes.Black,new XRect(0, 0, pdfPage.Width, pdfPage.Height), XStringFormats.TopCenter);
            
            int y = 40;
            fileContent.ForEach((line) =>
            {
                gfx.DrawString(line, new XFont("Verdana", 16, XFontStyle.Regular), XBrushes.Black, new XRect(0, y, pdfPage.Width-10, pdfPage.Height), XStringFormats.TopLeft);
                y += 20;
            });

            MemoryStream memoryStream = new();
            pdfdocument.Save(memoryStream);
            return memoryStream.ToArray();
        }
    }
}