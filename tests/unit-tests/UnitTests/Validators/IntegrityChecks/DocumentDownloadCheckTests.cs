using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
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
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Document Download Check, No document download type configured for form 'test-name'")));
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

            var check = new DocumentDownloadCheck();

            // Act
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Document Download Check, Unknown document download type configured for form 'test-name'")));
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
            var check = new DocumentDownloadCheck();

            // Act
            var result = check.Validate(schema);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Messages.Any(_ => _.Equals($"FAILURE - Document Download Check, Unknown document download type configured for form 'test-name'")));
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
            var check = new DocumentDownloadCheck();

            // Act
            var result = check.Validate(schema);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}