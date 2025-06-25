using Elasticsearch.Net;
using form_builder.Enum;
using form_builder.Helpers.DocumentCreation;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

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

		public byte[] CreateHtmlDocument(List<string> fileContent, string formName, string answersTitle)
		{
			using (var stream = new MemoryStream())
			{
				var objStreamWriter = new StreamWriter(stream);
				objStreamWriter.WriteLine("<div>");

				if (answersTitle is null)
				{
					objStreamWriter.WriteLine($"<h2>Submitted answers for {formName}</h2>");
				}
				else
				{
					objStreamWriter.WriteLine($"<h2>{answersTitle}</h2>");
				}
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

		public byte[] CreatePdfDocument(List<string> fileContent, string formName, string answersTitle)
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

			string header = string.Empty;

			if (answersTitle is null)
			{
				header = $"Submitted answers for {formName}";
			}
			else
			{
				header = $"{answersTitle}";
			}

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
	}
}