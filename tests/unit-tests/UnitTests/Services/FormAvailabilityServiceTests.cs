using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Properties.EnabledForProperties;
using form_builder.Providers.EnabledFor;
using form_builder.Restrictions;
using form_builder.Services.FormAvailabilityService;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class FormAvailabilityServicsTests
    {
        private readonly FormAvailabilityService _service;
        private readonly Mock<IEnabledForProvider> _mockEnabledFor = new();
        private readonly Mock<IEnumerable<IEnabledForProvider>> _mockEnabledForProviders = new();
        private readonly List<IFormAccessRestriction> _formAccessRestrictions = new List<IFormAccessRestriction>();

        public FormAvailabilityServicsTests()
        {
            _mockEnabledFor.Setup(_ => _.Type)
                .Returns(EEnabledFor.TimeWindow);
            _mockEnabledFor.Setup(_ => _.IsAvailable(It.IsAny<EnabledForBase>()))
                .Returns(true);

            var enabledForItems = new List<IEnabledForProvider> { _mockEnabledFor.Object };
            _mockEnabledForProviders
                .Setup(m => m.GetEnumerator())
                .Returns(() => enabledForItems.GetEnumerator());

            _service = new FormAvailabilityService(_mockEnabledForProviders.Object, _formAccessRestrictions);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenNoEnvironmentAvailabilitiesAreSpecified()
        {
            // Arrange
            var formSchema = new FormSchema();

            // Act
            var result = _service.IsAvailable(formSchema.EnvironmentAvailabilities, "Int");

            // Assert
            Assert.True(result);
            _mockEnabledFor.Verify(_ => _.IsAvailable(It.IsAny<EnabledForBase>()), Times.Never);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsNotSpecified()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("prod", false)
                .Build();

            // Act
            var result = _service.IsAvailable(formSchema.EnvironmentAvailabilities, "Int");

            // Assert
            Assert.True(result);
            _mockEnabledFor.Verify(_ => _.IsAvailable(It.IsAny<EnabledForBase>()), Times.Never);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_True_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsTrue()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("Int", true)
                .Build();

            // Act
            var result = _service.IsAvailable(formSchema.EnvironmentAvailabilities, "Int");

            // Assert
            Assert.True(result);
            _mockEnabledFor.Verify(_ => _.IsAvailable(It.IsAny<EnabledForBase>()), Times.Never);
        }

        [Fact]
        public void IsAvailable_ShouldReturn_False_WhenRequestedEnvironmentAvailabilitiesIsSpecified_And_IsAvailableEqualsFalse()
        {
            // Arrange
            var formSchema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("Int", false)
                .Build();

            // Act
            var result = _service.IsAvailable(formSchema.EnvironmentAvailabilities, "Int");

            // Assert
            Assert.False(result);
            _mockEnabledFor.Verify(_ => _.IsAvailable(It.IsAny<EnabledForBase>()), Times.Never);
        }

        [Fact]
        public void IsAvailable_Should_Call_EnabledForProvider_When_EnvironmentAvailability_Contains_EnabledFor_Option()
        {
            // Arrange
            var enabledForTimeWindow = new List<EnabledForBase> {
                new EnabledForBase {
                    Type = EEnabledFor.TimeWindow,
                    Properties = new EnabledForProperties {
                        Start = DateTime.Now.AddDays(-1),
                        End = DateTime.Now.AddDays(+1)
                    }
                }
            };

            var formSchema = new FormSchemaBuilder()
                .WithEnvironmentAvailability("Int", false, enabledForTimeWindow)
                .Build();

            // Act
            _service.IsAvailable(formSchema.EnvironmentAvailabilities, "Int");

            // Assert
            _mockEnabledFor.Verify(_ => _.IsAvailable(It.IsAny<EnabledForBase>()), Times.Once);
        }

        [Fact]
        public void HaveFormAccessPreRequirsitesBeenMet_ShouldReturn_True_If_No_RestrictionsProvided()
        {
            var formSchema = new FormSchemaBuilder().Build(); 
            
            var result = _service.HaveFormAccessPreRequirsitesBeenMet(formSchema);

            Assert.True(result);
        }

        
        [Fact]
        public void HaveFormAccessPreRequirsitesBeenMet_ShouldReturn_True_If_All_RestrictionsProvided_Return_False()
        {
            var formSchema = new FormSchemaBuilder().Build(); 
            var firstMockRestriction = new Mock<IFormAccessRestriction>();
            firstMockRestriction.Setup(_ => _.IsRestricted(It.IsAny<FormSchema>())).Returns(false);
            _formAccessRestrictions.Add(firstMockRestriction.Object);

            var secondMockRestriction = new Mock<IFormAccessRestriction>();
            secondMockRestriction.Setup(_ => _.IsRestricted(It.IsAny<FormSchema>())).Returns(false);
            _formAccessRestrictions.Add(secondMockRestriction.Object);

            var result = _service.HaveFormAccessPreRequirsitesBeenMet(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void HaveFormAccessPreRequirsitesBeenMet_ShouldReturn_False_If_Any_RestrictionsProvided_Return_True()
        {
            var formSchema = new FormSchemaBuilder().Build(); 
            var firstMockRestriction = new Mock<IFormAccessRestriction>();
            firstMockRestriction.Setup(_ => _.IsRestricted(It.IsAny<FormSchema>())).Returns(false);
            _formAccessRestrictions.Add(firstMockRestriction.Object);

            var secondMockRestriction = new Mock<IFormAccessRestriction>();
            secondMockRestriction.Setup(_ => _.IsRestricted(It.IsAny<FormSchema>())).Returns(true);
            _formAccessRestrictions.Add(secondMockRestriction.Object);

            var result = _service.HaveFormAccessPreRequirsitesBeenMet(formSchema);

            Assert.False(result);
        }
    }
}
