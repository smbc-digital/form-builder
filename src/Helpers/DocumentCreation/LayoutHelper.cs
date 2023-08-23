using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore;

namespace form_builder.Helpers.DocumentCreation;

public class LayoutHelper
{
    private readonly PdfDocument _document;
    private readonly XUnit _topPosition;
    private readonly XUnit _bottomMargin;
    private XUnit _currentPosition;

    public LayoutHelper(PdfDocument document, XUnit topPosition, XUnit bottomMargin)
    {
        _document = document;
        _topPosition = topPosition;
        _bottomMargin = bottomMargin;
        _currentPosition = topPosition;
        CreatePage();
    }

    public XUnit GetLinePosition(XUnit requestedHeight)
    {
        return GetLinePosition(requestedHeight, -1f);
    }

    public XUnit GetLinePosition(XUnit requestedHeight, XUnit requiredHeight)
    {
        XUnit required = requiredHeight == -1f ? requestedHeight : requiredHeight;
        if (_currentPosition + required > _bottomMargin)
            CreatePage();
        XUnit result = _currentPosition;
        _currentPosition += requestedHeight;
        return result;
    }

    public XGraphics Gfx { get; private set; }
    public PdfPage Page { get; private set; }
    public XTextFormatter TextFormatter { get; private set; }

    private void CreatePage()
    {
        Page = _document.AddPage();
        Page.Size = PageSize.A4;
        Page.TrimMargins.All = 25;
        Gfx = XGraphics.FromPdfPage(Page);
        TextFormatter = new XTextFormatter(Gfx)
        {
            Alignment = XParagraphAlignment.Left
        };
        _currentPosition = _topPosition;
    }
}
