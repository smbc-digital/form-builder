using System.Collections.Generic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;
using Elements = form_builder.Models.Elements;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Element
{
    public class AddAnotherElementCheckTests
    {
        private readonly AddAnotherElementCheck _integrityCheck = new();

        [Fact]
        public void ValidateAddAnotherElement_NullProperties_ShouldAddError()
        {
            // Arrange
            var element = new Elements.Element() { Type = EElementType.AddAnother };

            // Act
            var result = _integrityCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Count.Equals(1));
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void ValidateAddAnotherElement_NullElements_ShouldAddError()
        {
            // Arrange
            var element = new Elements.Element()
            {
                Type = EElementType.AddAnother,
                Properties = new BaseProperty()
                {
                    Elements = null
                }
            };

            // Act
            var result = _integrityCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Count.Equals(1));
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData(EElementType.AddAnother)]
        [InlineData(EElementType.Name)]
        [InlineData(EElementType.H1)]
        [InlineData(EElementType.H2)]
        [InlineData(EElementType.H3)]
        [InlineData(EElementType.H4)]
        [InlineData(EElementType.H5)]
        [InlineData(EElementType.H6)]
        [InlineData(EElementType.HR)]
        [InlineData(EElementType.Img)]
        [InlineData(EElementType.P)]
        [InlineData(EElementType.OL)]
        [InlineData(EElementType.UL)]
        [InlineData(EElementType.Address)]
        [InlineData(EElementType.AddressManual)]
        [InlineData(EElementType.Booking)]
        [InlineData(EElementType.Button)]
        [InlineData(EElementType.Declaration)]
        [InlineData(EElementType.DocumentDownload)]
        [InlineData(EElementType.DocumentUpload)]
        [InlineData(EElementType.FileUpload)]
        [InlineData(EElementType.InlineAlert)]
        [InlineData(EElementType.Link)]
        [InlineData(EElementType.Map)]
        [InlineData(EElementType.MultipleFileUpload)]
        [InlineData(EElementType.Numeric)]
        [InlineData(EElementType.Organisation)]
        [InlineData(EElementType.Reusable)]
        [InlineData(EElementType.Select)]
        [InlineData(EElementType.Span)]
        [InlineData(EElementType.Street)]
        [InlineData(EElementType.Summary)]
        [InlineData(EElementType.UploadedFilesSummary)]
        [InlineData(EElementType.Warning)]
        public void ValidateAddAnotherElement_InvalidElementType_ShouldAddError(EElementType elementType)
        {
            // Arrange
            var element = new Elements.Element()
            {
                Type = EElementType.AddAnother,
                Properties = new BaseProperty()
                {
                    Elements = new List<IElement>()
                    {
                        new Elements.Element() { Type = elementType }
                    }
                }
            };

            // Act
            var result = _integrityCheck.Validate(element);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Count.Equals(1));
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Theory]
        [InlineData(EElementType.Textbox)]
        [InlineData(EElementType.Textarea)]
        [InlineData(EElementType.DateInput)]
        [InlineData(EElementType.DatePicker)]
        [InlineData(EElementType.Radio)]
        [InlineData(EElementType.Checkbox)]
        [InlineData(EElementType.TimeInput)]
        public void ValidateAddAnotherElement_ValidElementType_Should_Not_AddError(EElementType elementType)
        {
            // Arrange
            var element = new Elements.Element()
            {
                Type = EElementType.AddAnother,
                Properties = new BaseProperty()
                {
                    Elements = new List<IElement>()
                    {
                        new Elements.Element() { Type = elementType }
                    }
                }
            };

            // Act
            var result = _integrityCheck.Validate(element);

            // Assert
            Assert.True(result.IsValid);
            Assert.True(result.Messages.Count.Equals(0));
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}
