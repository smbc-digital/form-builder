using form_builder.Constants;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Form;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class PaymentCallbackCheckTests
    {
        [Fact]
        public void PaymentCallbackCheck_Validate_AddsFailureMessage_If_CallbackFailureContactNumber_Empty_And_ProcessPaymentCallbackResponse_True()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                ProcessPaymentCallbackResponse = true
            };

            PaymentCallbackCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.False(result.IsValid);
            Assert.Collection(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void PaymentCallbackCheck_Validate_DoesNotAddFailureMessage_If_CallbackFailureContactNumber_Empty_And_ProcessPaymentCallbackResponse_False()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                ProcessPaymentCallbackResponse = false
            };

            PaymentCallbackCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
            Assert.Empty(result.Messages);
        }

        [Fact]
        public void PaymentCallbackCheck_Validate_DoesNotAddFailureMessage_If_CallbackFailureContactNumber_NotEmpty_And_ProcessPaymentCallbackResponse_False()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                ProcessPaymentCallbackResponse = false,
                CallbackFailureContactNumber = "123"
            };

            PaymentCallbackCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
            Assert.Empty(result.Messages);
        }

        [Fact]
        public void PaymentCallbackCheck_Validate_DoesNotAddFailureMessage_If_CallbackFailureContactNumber_NotEmpty_And_ProcessPaymentCallbackResponse_True()
        {
            // Arrange
            var schema = new FormSchema
            {
                FormName = "test-form",
                ProcessPaymentCallbackResponse = true,
                CallbackFailureContactNumber = "123"
            };

            PaymentCallbackCheck check = new();

            // Act
            var result = check.Validate(schema);
            Assert.True(result.IsValid);
            Assert.Empty(result.Messages);
        }
    }
}
