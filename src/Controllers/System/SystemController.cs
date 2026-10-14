namespace form_builder.Controllers.System;

[Route("[Controller]")]
[TokenAuthentication]
public class SystemController(ILogger<SystemController> logger, ISchemaProvider schemaProvider)
    : Controller
{
    [HttpPatch]
    [IgnoreAntiforgeryToken]
    [Route("index-schemas")]
    public async Task<IActionResult> IndexSchemas() => Ok(await schemaProvider.IndexSchema());
}