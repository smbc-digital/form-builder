using System.Linq;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class GeneratedIdConfigurationTests
    {
        [Fact]
        public void CheckGeneratedIdConfiguration_Throw_ApplicationException_WhenGeneratedReferenceNumberMapping_IsNullOrEmpty()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                GenerateReferenceNumber = true,
                ReferencePrefix = "TEST"
            };
            
            var check = new GeneratedIdConfigurationCheck();

            // Act
            var result = check.Validate(schema);
            Assert.Equal($"FAILURE - Generated Id Configuration Check, 'GeneratedReferenceNumberMapping' and 'ReferencePrefix' must both have a value in form test-form", result.Messages.First());
        }

        [Fact]
        public void CheckGeneratedIdConfiguration_Throw_ApplicationException_WhenReferencePrefix_IsNullOrEmpty()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                GenerateReferenceNumber = true,
                GeneratedReferenceNumberMapping = "CaseReference"
            };
            
            var check = new GeneratedIdConfigurationCheck();

            // Act
            var result = check.Validate(schema);
            Assert.Equal($"FAILURE - Generated Id Configuration Check, 'GeneratedReferenceNumberMapping' and 'ReferencePrefix' must both have a value in form test-form", result.Messages.First());
        }

        [Fact]
        public void CheckGeneratedIdConfiguration_DoesNotThrow_ApplicationException_GeneratedIdConfig_IsCorrect()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                GenerateReferenceNumber = true,
                GeneratedReferenceNumberMapping = "CaseReference",
                ReferencePrefix = "TEST"
            };
            
            var check = new GeneratedIdConfigurationCheck();

            // Act
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
        }
    }
}