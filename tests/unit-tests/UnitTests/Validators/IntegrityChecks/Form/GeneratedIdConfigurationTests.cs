using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Form;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
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
                ReferencePrefix = "TEST",
                Pages = new List<Page>()
            };

            GeneratedIdConfigurationCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void CheckGeneratedIdConfiguration_Throw_ApplicationException_WhenReferencePrefix_IsNullOrEmpty()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                GenerateReferenceNumber = true,
                GeneratedReferenceNumberMapping = "CaseReference",
                Pages = new List<Page>()
            };

            GeneratedIdConfigurationCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
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
                ReferencePrefix = "TEST",
                Pages = new List<Page>()
            };

            GeneratedIdConfigurationCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
            Assert.DoesNotContain(IntegrityChecksConstants.FAILURE, result.Messages);
        }

        [Fact]
        public void CheckGeneratedIdConfiguration_Throw_ApplicationException_WhenGenerateReferenceNumber_IsFalse_And_SubmitWithoutSubmission_IsUsed()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                GenerateReferenceNumber = false,
                GeneratedReferenceNumberMapping = "CaseReference",
                ReferencePrefix = "TEST",
                Pages = new List<Page>
                {
                    new ()
                    {
                        Behaviours = new List<Behaviour>
                        {
                            new ()
                            {
                                BehaviourType = EBehaviourType.SubmitWithoutSubmission
                            }
                        }
                    }
                }
            };

            GeneratedIdConfigurationCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }
    }
}