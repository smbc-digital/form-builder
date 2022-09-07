using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Properties.EnabledForProperties;
using form_builder.Validators.IntegrityChecks.Form;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form
{
    public class EnabledForTimeWindowCheckTests
    {
        private readonly EnabledForTimeWindowCheck _integrityCheck = new();

        [Fact]
        public void Validate_ShouldReturnFalse_When_TypeIsUnknown()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.Unknown
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Single(result.Messages);
            Assert.False(result.IsValid);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}EnabledFor Check, Unknown EnabledFor type configured.", result.Messages.FirstOrDefault());
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_Properties_AreNull_ForTimeWindow()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Single(result.Messages);
            Assert.False(result.IsValid);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}EnabledFor Check, EnabledFor Properties must be defined", result.Messages.FirstOrDefault());
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_Start_And_End_AreNotSet()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow,
                    Properties = new EnabledForProperties()
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Single(result.Messages);
            Assert.False(result.IsValid);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}EnabledFor Check, Start and End cannot be Min and Max Value.", result.Messages.FirstOrDefault());
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_Start_IfAfterEnd_Date()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow,
                    Properties = new EnabledForProperties
                    {
                        Start = DateTime.Now.AddDays(1),
                        End = DateTime.Now
                    }
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Single(result.Messages);
            Assert.False(result.IsValid);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}EnabledFor Check, Start Date cannot be after End Date.", result.Messages.FirstOrDefault());
        }

        [Fact]
        public void Validate_ShouldReturnFalse_When_End_IfBeforeStart_Date()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow,
                    Properties = new EnabledForProperties
                    {
                        Start = DateTime.Now,
                        End = DateTime.Now.AddDays(-1),
                    }
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Single(result.Messages);
            Assert.False(result.IsValid);
            Assert.Equal($"{IntegrityChecksConstants.FAILURE}EnabledFor Check, Start Date cannot be after End Date.", result.Messages.FirstOrDefault());
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenTimeWindow_IsValid_With_Both_Start_And_End_Supplied()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow,
                    Properties = new EnabledForProperties
                    {
                        Start =  DateTime.Now,
                        End =  DateTime.Now.AddDays(1)
                    }
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Empty(result.Messages);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenTimeWindow_IsValid_When_Start_Supplied()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow,
                    Properties = new EnabledForProperties
                    {
                        Start = DateTime.Now
                    }
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Empty(result.Messages);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ShouldReturnTrue_WhenTimeWindow_IsValid_When_End_Supplied()
        {
            // Arrange
            var enabledForBaseModel = new List<EnabledForBase>
            {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow,
                    Properties = new EnabledForProperties
                    {
                        End = DateTime.Now
                    }
                }
            };

            var schema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("int", true, enabledForBaseModel)
                .Build();

            // Act
            var result = _integrityCheck.Validate(schema);
            Assert.Empty(result.Messages);
            Assert.True(result.IsValid);
        }
    }
}