using Amazon.SimpleEmail.Model;
using Elasticsearch.Net;
using form_builder.Enum;
using form_builder.Helpers.DocumentCreation;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using FileFormat.Words;
using FileFormat.Words.IElements;
using form_builder.Models;

namespace form_builder.Providers.DocumentCreation.Generic
{
    public class TextFileDocumentCreator : IDocumentCreation
    {
        public EProviderPriority Priority => EProviderPriority.High;
        public EDocumentType DocumentType => EDocumentType.Txt;
        public byte[] CreateDocument(List<string> fileContent)
        {
            string cleanLine = string.Empty;
            using var stream = new MemoryStream();
            using (var objStreamWriter = new StreamWriter(stream))
            {
                fileContent.ForEach((line) =>
                        {
                            cleanLine = line.Replace("<br/>", "").Replace("<b>", "").Replace("</b>", ": ");
                            objStreamWriter.WriteLine(cleanLine);
                        });
                objStreamWriter.Flush();
                objStreamWriter.Close();
            }

            return stream.ToArray();
        }

        public byte[] CreateHtmlDocument(List<string> fileContent, string formName)
        {
            using (var stream = new MemoryStream())
            {
                var objStreamWriter = new StreamWriter(stream);
                objStreamWriter.WriteLine("<div>");
                objStreamWriter.WriteLine($"<h2>Submitted answers for {formName}</h2>");
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

        public byte[] CreatePdfDocument(List<string> fileContent, string formName)
        {
            PdfDocument pdfDocument = new();
            LayoutHelper layoutHelper = new(pdfDocument, XUnit.FromPoint(10), XUnit.FromCentimeter(29.7 - 1));
            XStringFormat format = new()
            {
                LineAlignment = XLineAlignment.Near,
                Alignment = XStringAlignment.Near
            };

            XFont fontBold = new("Verdana", 16, XFontStyle.Bold);
            XFont fontRegular = new("Verdana", 16, XFontStyle.Regular);

            string header = $"Submitted answers for {formName}";
            XUnit headerTop = layoutHelper.GetLinePosition(
                GetRequestedHeightBasedOnLineLength(header.Length, 45, 15));

            layoutHelper.TextFormatter.DrawString(header, fontBold, XBrushes.Black,
                new XRect(7.5, headerTop, layoutHelper.Page.Width - 15, layoutHelper.Page.Height), format);

            for (int line = 0; line < fileContent.Count; line++)
            {
                // FileContent lines should come in groups of 3 - question, answer, empty line. If the previous line is empty then we use the fontBold for the question
                if (line.Equals(0) || (line >= 1 && fileContent[line - 1].Length.Equals(0)))
                {
                    XUnit top = layoutHelper.GetLinePosition(
                        GetRequestedHeightBasedOnLineLength(fileContent[line].Length, 20, 17));

                    layoutHelper.TextFormatter.DrawString(fileContent[line], fontBold, XBrushes.Black,
                        new XRect(7.5, top, layoutHelper.Page.Width - 15, layoutHelper.Page.Height), format);
                }
                else
                {
                    XUnit top = layoutHelper.GetLinePosition(
                        GetRequestedHeightBasedOnLineLength(fileContent[line].Length, 25, 15));

                    layoutHelper.TextFormatter.DrawString(fileContent[line], fontRegular, XBrushes.Black,
                        new XRect(7.5, top, layoutHelper.Page.Width - 15, layoutHelper.Page.Height), format);
                }
            }

            MemoryStream memoryStream = new();
            pdfDocument.Save(memoryStream);
            return memoryStream.ToArray();
        }

        //public byte[] CreateWordDocument(List<string> fileContent, string formName)
        //{
        //    PdfDocument pdfDocument = new();
        //    LayoutHelper layoutHelper = new(pdfDocument, XUnit.FromPoint(10), XUnit.FromCentimeter(29.7 - 1));
        //    XStringFormat format = new()
        //    {
        //        LineAlignment = XLineAlignment.Near,
        //        Alignment = XStringAlignment.Near
        //    };

        //    XFont fontBold = new("Verdana", 16, XFontStyle.Bold);
        //    XFont fontRegular = new("Verdana", 16, XFontStyle.Regular);

        //    string header = $"Submitted answers for {formName}";
        //    XUnit headerTop = layoutHelper.GetLinePosition(
        //        GetRequestedHeightBasedOnLineLength(header.Length, 45, 15));

        //    layoutHelper.TextFormatter.DrawString(header, fontBold, XBrushes.Black,
        //        new XRect(7.5, headerTop, layoutHelper.Page.Width - 15, layoutHelper.Page.Height), format);

        //    for (int line = 0; line < fileContent.Count; line++)
        //    {
        //        // FileContent lines should come in groups of 3 - question, answer, empty line. If the previous line is empty then we use the fontBold for the question
        //        if (line.Equals(0) || (line >= 1 && fileContent[line - 1].Length.Equals(0)))
        //        {
        //            XUnit top = layoutHelper.GetLinePosition(
        //                GetRequestedHeightBasedOnLineLength(fileContent[line].Length, 20, 17));

        //            layoutHelper.TextFormatter.DrawString(fileContent[line], fontBold, XBrushes.Black,
        //                new XRect(7.5, top, layoutHelper.Page.Width - 15, layoutHelper.Page.Height), format);
        //        }
        //        else
        //        {
        //            XUnit top = layoutHelper.GetLinePosition(
        //                GetRequestedHeightBasedOnLineLength(fileContent[line].Length, 25, 15));

        //            layoutHelper.TextFormatter.DrawString(fileContent[line], fontRegular, XBrushes.Black,
        //                new XRect(7.5, top, layoutHelper.Page.Width - 15, layoutHelper.Page.Height), format);
        //        }
        //    }

        //    MemoryStream memoryStream = new();
        //    pdfDocument.Save(memoryStream);
        //    return memoryStream.ToArray();
        //}

        private int GetRequestedHeightBasedOnLineLength(int lineLength, int baseHeight, int incrementAmount)
        {
            // Uses an estimate of 60 characters per line to calculate how much height to add to the _currentPosition in the LayoutHelper
            while (lineLength > 60)
            {
                baseHeight += incrementAmount;
                lineLength -= 60;
            }

            return baseHeight;
        }

        public byte[] MakeWordAttachment(List<string> answerList, string formName)
        {
            Document doct = new();
            FileFormat.Words.Body bodyt = new(doct);

            string docTitle = formName;
            docTitle += DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");

            string filename = $"c:/temp/{docTitle}.docx";

            var rowNumber = 0;

            var tableStyles = doct.GetElementStyles().TableStyles;

            Table table = new(answerList.Count, 1)
            {
                Style = tableStyles[1]
            };
            table.Column.Width = 3000;


            Paragraph para = new()
            {
                Style = Headings.Heading1,
            };

            para.AddRun(new Run
            {
                Text = formName
            });

            bodyt.AppendChild(para);

            foreach (var row in answerList)
            {
                rowNumber++;
                para = new Paragraph();
                para.AddRun(new Run
                {
                    Text = row.ToString(),
                    Bold = true,
                    FontSize = 16
                });

                bodyt.AppendChild(para);
            }
            
            rowNumber = 0;
            
            foreach (var row in table.Rows)
            {
                rowNumber++;
                if (answerList[rowNumber - 1].Length is not 0)
                {
                    para = new Paragraph();
                    para.AddRun(new Run
                    {
                        Text = answerList[rowNumber - 1].ToString(),
                        Bold = true,
                        FontSize = 16
                    });
                    row.Cells[0].Paragraphs.Add(para);                    
                }
            }

            bodyt.AppendChild(table);

            MemoryStream memoryStream = new();
            doct.Save(memoryStream);
            return memoryStream.ToArray();
        }
    }
}