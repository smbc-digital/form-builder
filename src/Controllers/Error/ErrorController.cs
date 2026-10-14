namespace form_builder.Controllers.Error;

[Route("error")]
public class ErrorController : Controller
{
    public ActionResult Index() => View();

    [Route("/not-found")]
    new public ActionResult NotFound() => View();
}