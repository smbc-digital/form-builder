using System;
using Microsoft.AspNetCore.Mvc;
using form_builder.Providers.SchemaProvider;
using Microsoft.Extensions.Logging;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;
using System.Threading.Tasks;

namespace form_builder.Controllers
{
    [Route("[Controller]")]
    [TokenAuthentication]
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
        [IgnoreAntiforgeryToken]
        [Route("index-schemas")]
        public async Task<IActionResult> IndexSchemas() => Ok(await _schemaProvider.IndexSchema());
    }
}