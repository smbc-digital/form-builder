using form_builder.Constants;
using form_builder.Enum;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class DcoumentDownloadCheckTests
    {
        
        [Fact]
        public void DocumentDownloadCheck_IsNotValid_WhenFormSchemaContains_NoDocumentTypes_WhenDocumentDownload_True()
        {
            // Arrange
            var schema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithName("test-name")
                .Build();

            var check = new DocumentDownloadCheck();

            // Act
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void DocumentDownloadCheck_IsNotValid_WhenFormSchemaContains_UnknownDocumentType_WhenDocumentDownload_True()
        {
            // Arrange
            var schema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Unknown)
                .WithName("test-name")
                .Build();

            DocumentDownloadCheck check = new();

            // Act
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldThrowApplicationException_WhenFormSchemaContains_UnknownDocumentType_InList_WhenDocumentDownload_True()
        {
            // Arrange
            var schema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Unknown)
                .WithName("test-name")
                .Build();

            // Act
            DocumentDownloadCheck check = new();

            // Act
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void CheckForDocumentDownload_ShouldNotThrowApplicationException_WhenValidFormSchema_ForDocumentDownload()
        {
            // Arrange
            var schema = new FormSchemaBuilder()
                .WithDocumentDownload(true)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Txt)
                .WithDocumentType(EDocumentType.Txt)
                .Build();

            // Act
            DocumentDownloadCheck check = new();

            // Act
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }
    }
}