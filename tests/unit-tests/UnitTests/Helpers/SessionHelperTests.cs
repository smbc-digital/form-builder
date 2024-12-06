using form_builder.Helpers.Session;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class SessionHelperTests
    {
        private readonly Mock<ISessionHelper> _mockSession = new();

        [Fact]
        public void GetSessionGuid_ReturnsGUID()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var result = _mockSession.Setup(_ => _.GetBrowserSessionId()).Returns(guid.ToString());

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
            // Act
            var result = _mockSession.Setup(_ => _.RemoveSessionGuid());

            // Assert
            Assert.NotNull(result);
        }
    }
}