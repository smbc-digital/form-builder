using Xunit;
using form_builder.Controllers.HealthCheck;
using Microsoft.AspNetCore.Mvc;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HealthCheckControllerTests
    {
        private HealthCheckController _healthCheckController;

        public HealthCheckControllerTests()
        {
            _healthCheckController = new HealthCheckController();
        }

        [Fact]
        public void HealthCheckShouldReturnHealthCheck()
        {
            // Act
            var response = _healthCheckController.Get();
            var result = response as OkObjectResult;

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(response);
            Assert.NotNull(result.Value);
        }
    }
}
