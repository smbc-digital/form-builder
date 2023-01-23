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

            XFont font = new XFont("Verdana", 20, XFontStyle.Bold);

            gfx.DrawString("Form Data", font, XBrushes.Black,new XRect(0, 0, pdfPage.Width, pdfPage.Height), XStringFormats.TopCenter);
            int y = 60;


            XFont bodyFont = new XFont("Verdana", 20, XFontStyle.Regular);
            fileContent.ForEach((line) =>
            {
                gfx.DrawString(line, bodyFont, XBrushes.Black, new XRect(0, y, pdfPage.Width, pdfPage.Height), XStringFormats.TopLeft);
                y += 30;
            });
            MemoryStream memoryStream = new();

            pdfdocument.Save(memoryStream);
            return memoryStream.ToArray();
             
        }

    }
}