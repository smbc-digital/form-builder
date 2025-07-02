using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        public ActionResult Index() => View();

        [Route("/not-found")]
        new public ActionResult NotFound() => View();

		[Route("/time-out")]
		new public ActionResult TimeOut() => View();
	}
}