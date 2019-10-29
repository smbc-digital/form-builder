using System;
using System.Collections.Generic;
using System.Text;
using form_builder.Configuration;
using form_builder.Controllers;
using form_builder.Providers;
using form_builder.Validators;
using form_builder.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private readonly Mock<ICacheProvider> _cacheProvider = new Mock<ICacheProvider>();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private ISchemaProvider _schemaProvider = new LocalFileSchemaProvider();
        private readonly Mock<ITempDataProvider> _tempDataProvider = new Mock<ITempDataProvider>();
        private readonly Mock<IServiceProvider> _serviceProvider = new Mock<IServiceProvider>();
        private readonly Mock<IOptions<DisallowedAnswerKeysConfiguration>> _disallowedKeys = new Mock<IOptions<DisallowedAnswerKeysConfiguration>>();
        private readonly Mock<IRazorPageFactoryProvider> _razorFactoryProvider = new Mock<IRazorPageFactoryProvider>();
        private readonly Mock<IRazorPageActivator> _razorPageActivator = new Mock<IRazorPageActivator>();
        private readonly Mock<IRazorViewEngine> _razorViewEngine = new Mock<IRazorViewEngine>();
        private readonly Mock<IOptions<RazorViewEngineOptions>> _options = new Mock<IOptions<RazorViewEngineOptions>>();
        private readonly Mock<ILoggerFactory> _logger = new Mock<ILoggerFactory>();
        private readonly Mock<System.Diagnostics.DiagnosticSource> _diagnosticSource = new Mock<System.Diagnostics.DiagnosticSource>();
        
        public HomeControllerTest()
        {
            
            var _viewRender = new ViewRender(_razorViewEngine.Object, _tempDataProvider.Object, _serviceProvider.Object);
            _homeController = new HomeController(_cacheProvider.Object, _validators.Object, _schemaProvider, _viewRender, _disallowedKeys.Object);

        }

        [Fact]
        public void Index_Should_Return_Index()
        {
            Assert.IsType<ViewResult>(_homeController.Index());
        }

        


    }
}
