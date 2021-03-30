using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Validators.IntegrityChecks.Behaviours;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators.IntegrityChecks
{
    public class SubmitSlugsHaveAllPropertiesCheckTests
    {
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        public SubmitSlugsHaveAllPropertiesCheckTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName)
                .Returns("local");
        }
        [Fact]
        public void SubmitSlugsHaveAllPropertiesCheck_IsNotValid_WhenAuthTokenIsNullOrEmptyAndBehaviourTypeIsNotSubmitPowerAutomate()
        {
            // Arrange
            var submitSlugs = new SubmitSlug
            {
                URL = "test",
                Environment = "local"
            };

            var behaviours = new List<Behaviour>
            {
                new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build()
        };
            

            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(behaviours);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void SubmitSlugsHaveAllProperties_IsNotValue_WhenUrlIsNull()
        {
            // Arrange
            var submitSlugs = new SubmitSlug
            {
                Environment = "local",
                AuthToken = "this is auth token"
            };

            var behaviours = new List<Behaviour>
            {
                new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build()
            };
          
            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(behaviours);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void SubmitSlugsHaveAllProperties_IsNotValid_WhenUrlIsEmpty()
        {
            // Arrange
            var submitSlugs = new SubmitSlug
            {
                AuthToken = "this is auth token",
                URL = "",
                Environment = "local",
            };

            var behaviours = new List<Behaviour>
            {
                new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build()
            };
        
            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(behaviours);

            // Assert
            Assert.False(result.IsValid);
            Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
        }

        [Fact]
        public void CheckSubmitSlugsHaveAllProperties_IsValid_WhenAuthTokenAndUrlAreNotNullOrEmpty()
        {
            // Arrange
            var submitSlugs = new SubmitSlug
            {
                AuthToken = "this is auth token",
                URL = "test"
            };

            var behaviours = new List<Behaviour>
            {
                new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .WithSubmitSlug(submitSlugs)
                .Build()
            };
           
            // Act
            var check = new SubmitSlugsHaveAllPropertiesCheck(_mockHostingEnv.Object);
            var result = check.Validate(behaviours);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}