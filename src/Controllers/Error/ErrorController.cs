using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        public ActionResult Index() => View();

        [Route("/not-found")]
        public ActionResult NotFound() => View();


        [Route("/service-unavailable")]
        public ActionResult ServiceUnavailable() => View();
    }
}