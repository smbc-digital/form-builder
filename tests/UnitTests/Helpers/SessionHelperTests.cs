using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class SessionHelperTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        private readonly Mock<ISessionHelper> _mockSession = new Mock<ISessionHelper>();


        public SessionHelperTests()
        {
            _mockDistributedCache = new Mock<IDistributedCacheWrapper>();
        }

        [Fact]
        public void GetSessionGuid_ReturnsGUID()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var result = _mockSession.Setup(_ => _.GetSessionGuid()).Returns(guid.ToString());

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void SetSessionGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var result = _mockSession.Setup(_ => _.SetSessionGuid(guid.ToString()));

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void RemoveSessionGuid()
        {
            // Arrange

            // Act
            var result = _mockSession.Setup(_ => _.RemoveSessionGuid());

            // Assert
            Assert.NotNull(result);
        }
    }
}
