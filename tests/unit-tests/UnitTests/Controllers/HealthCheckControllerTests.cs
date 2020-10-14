using form_builder.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HealthCheckControllerTests
    {
        private readonly HealthCheckController _healthCheckController;

        public HealthCheckControllerTests()
        {
            _healthCheckController = new HealthCheckController();
        }

        [Fact]
        public void HealthCheckShouldReturnHealthCheck()
        {
            // Act
            var response = _healthCheckController.Get() as OkObjectResult;

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response);
            Assert.NotNull(response.Value);
        }
    }
}
