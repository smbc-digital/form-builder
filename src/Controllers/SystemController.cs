using System;
using Microsoft.AspNetCore.Mvc;
using form_builder.Providers.SchemaProvider;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace form_builder.Controllers
{
    [TokenAuthentication]
    [Route("{controller}")]
    public class SystemController : Controller
    {
        private ISchemaProvider _schemaProvider;

        private ILogger<SystemController> _logger;

        public SystemController(ILogger<SystemController> logger, ISchemaProvider schemaProvider)
        {
            _logger = logger;
            _schemaProvider = schemaProvider;
        }

        [HttpPatch]
        [Route("index-schemas")]
        public IActionResult IndexSchemas() => Ok(_schemaProvider.IndexSchema());
    }
}