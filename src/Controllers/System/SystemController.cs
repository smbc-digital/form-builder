using form_builder.Providers.SchemaProvider;
using Microsoft.AspNetCore.Mvc;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace form_builder.Controllers;

[Route("[Controller]")]
[TokenAuthentication]
public class SystemController(ILogger<SystemController> logger, ISchemaProvider schemaProvider)
    : Controller
{
    private ISchemaProvider _schemaProvider = schemaProvider;
    private ILogger<SystemController> _logger = logger;

    [HttpPatch]
    [IgnoreAntiforgeryToken]
    [Route("index-schemas")]
    public async Task<IActionResult> IndexSchemas() => Ok(await _schemaProvider.IndexSchema());
}