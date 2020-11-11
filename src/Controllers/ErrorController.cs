using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            Response.StatusCode = 500;
            return View();
        }

        [Route("/not-found")]
        new public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}