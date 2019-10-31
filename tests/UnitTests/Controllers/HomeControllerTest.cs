using System.Collections.Generic;
using form_builder.Configuration;
using form_builder.Controllers;
using form_builder.Providers;
using form_builder.Validators;
using form_builder.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Gateways;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private readonly Mock<ICacheProvider> _cacheProvider = new Mock<ICacheProvider>();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private ISchemaProvider _schemaProvider = new LocalFileSchemaProvider();       
        private readonly Mock<IOptions<DisallowedAnswerKeysConfiguration>> _disallowedKeys = new Mock<IOptions<DisallowedAnswerKeysConfiguration>>();       
        private readonly Mock<IOptions<RazorViewEngineOptions>> _options = new Mock<IOptions<RazorViewEngineOptions>>();
        private readonly Mock<ILoggerFactory> _logger = new Mock<ILoggerFactory>();
        private readonly Mock<System.Diagnostics.DiagnosticSource> _diagnosticSource = new Mock<System.Diagnostics.DiagnosticSource>();
        private readonly Mock<IViewRender> _viewRender = new Mock<IViewRender>();
        private readonly Mock<IGateway> _gateWay = new Mock<IGateway>();
        
        public HomeControllerTest()
        {
            
       
            _homeController = new HomeController(_cacheProvider.Object, _validators.Object, _schemaProvider,_viewRender.Object, _disallowedKeys.Object, _gateWay.Object);

        }

        [Fact]
        public void Index_Should_Return_Index()
        {
            Assert.IsType<ViewResult>(_homeController.Index());
        }
    }
}
