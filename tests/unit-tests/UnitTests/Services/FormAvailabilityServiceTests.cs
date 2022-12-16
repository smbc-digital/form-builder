using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Properties.EnabledForProperties;
using form_builder.Providers.EnabledFor;
using form_builder.Services.FormAvailabilityService;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class FormAvailabilityServicsTests
    {
        private readonly FormAvailabilityService _service;
        private readonly Mock<IEnabledForProvider> _mockEnabledFor = new();
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
        private readonly Mock<IEnumerable<IEnabledForProvider>> _mockEnabledForProviders = new();

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

            _service = new FormAvailabilityService(_mockEnabledForProviders.Object, _mockHttpContextAccessor.Object);
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
        public void HaveFormAccessPreRequirsitesBeenMet_ShouldReturn_True_If_No_Key_Is_Specified()
        {
            var formSchema = new FormSchemaBuilder().Build(); 
            
            var result = _service.HaveFormAccessPreRequirsitesBeenMet(formSchema);

            Assert.True(result);
        }

        [Fact]
        public void HaveFormAccessPreRequirsitesBeenMet_ShouldReturn_False_If_Key_Is_Specified_And_HttpContext_Contains_No_Values()
        {
            var mockHttpContext = new Mock<HttpContent>();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

           _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Query)
                            .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
            .WithSpecifiedKey("TestKey", "TestToken")
            .Build(); 
            
            var result = _service.HaveFormAccessPreRequirsitesBeenMet(formSchema);

            Assert.False(result);
        }
        
        [Fact]
        public void HaveFormAccessPreRequirsitesBeenMet_ShouldReturn_False_If_Key_Is_Specified_And_HttpContext_Contains_Incorrect_Values()
        {
            
            var mockHttpContext = new Mock<HttpContent>();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                { "TestKey", "IncorrectToken" }
            });

           _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Query)
                            .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
            .WithSpecifiedKey("TestKey", "TestToken")
            .Build(); 
            
            var result = _service.HaveFormAccessPreRequirsitesBeenMet(formSchema);

            Assert.False(result);
        }

        [Fact]
        public void HaveFormAccessPreRequirsitesBeenMet_ShouldReturn_True_If_Key_Is_Specified_And_HttpContext_Contains_Correct_Values()
        {
            
            var mockHttpContext = new Mock<HttpContent>();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                { "TestKey", "TestToken" }
            });

           _mockHttpContextAccessor.Setup(_ => _.HttpContext.Request.Query)
                            .Returns(queryCollection);

            var formSchema = new FormSchemaBuilder()
            .WithSpecifiedKey("TestKey", "TestToken")
            .Build(); 
            
            var result = _service.HaveFormAccessPreRequirsitesBeenMet(formSchema);

            Assert.True(result);
        }
    }


}
