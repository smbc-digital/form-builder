using form_builder.Helpers.DocumentCreation;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers;

public class LayoutHelperTests
{
    [Fact]
    public void LayoutHelper_ShouldAddPageOnInstantiatingClass()
    {
        // Arrange
        var pdfDocument = new PdfDocument();

        // Act
        var layoutHelper = new LayoutHelper(pdfDocument, XUnit.FromPoint(10), XUnit.FromCentimeter(29.7 - 1));

        // Assert
        Assert.Single(pdfDocument.Pages);
    }

    [Fact]
    public void GetLinePosition_ShouldAddPageIfRequiredHeightGreaterThanRemainingSpaceOnPage()
    {
        // Arrange
        var pdfDocument = new PdfDocument();
        var layoutHelper = new LayoutHelper(pdfDocument, XUnit.FromPoint(10), XUnit.FromCentimeter(29.7 - 1));

        // Act
        layoutHelper.GetLinePosition(new XUnit(810));

        // Assert
        Assert.Equal(2, pdfDocument.Pages.Count);
    }
}
