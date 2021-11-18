using form_builder.Builders;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Elements;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Element
{
    public class DocumentDownloadCheckTests
    {
        private readonly DocumentDownloadCheck _integrityCheck = new();

        [Fact]
        public void Validate_ShouldReturnInValid_IfDocumentTypeMissing()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DocumentDownload)
                .WithQuestionId("DocumentDownload")
                .Build();

            // Act & Assert
            var result = _integrityCheck.Validate(element);
            
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnInValid_IfDocumentTypeInvalid()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DocumentDownload)
                .WithQuestionId("DocumentDownload")
                .WithDocumentType(EDocumentType.Unknown)
                .Build();

            var result = _integrityCheck.Validate(element);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnValid_IfDocumentTypeSet()
        {
            // Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.DocumentDownload)
                .WithQuestionId("DocumentDownload")
                .WithDocumentType(EDocumentType.Txt)
                .Build();

            var result = _integrityCheck.Validate(element);

            Assert.True(result.IsValid);
        }
    }
}
