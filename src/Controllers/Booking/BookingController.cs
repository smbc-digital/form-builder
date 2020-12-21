using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Services.BookingService;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IPageHelper _pageHelper;
        private readonly ISessionHelper _sessionHelper;

        public BookingController(IBookingService bookingService, ISchemaFactory schemaFactory, IPageHelper pageHelper, ISessionHelper sessionHelper)
        {
            _bookingService = bookingService;
            _schemaFactory = schemaFactory;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
        }

        [HttpPost]
        [Route("booking/{form}/{path}/month")]
        public async Task<IActionResult> Index(
            string form,
            string path,
            Dictionary<string, string[]> formData)
        {
            var viewModel = formData.ToNormaliseDictionary(string.Empty);
            var queryParamters = Request.Query;
            var baseForm = await _schemaFactory.Build(form);

            if (baseForm == null)
                throw new ApplicationException($"Requested form '{form}' could not be found.");

            var page = baseForm.GetPage(_pageHelper, path);
            if (page == null)
                throw new ApplicationException($"Requested path '{path}' object could not be found for form '{form}'");

            var sessionGuid = _sessionHelper.GetSessionGuid();

            await _bookingService.ProcessMonthRequest(viewModel, baseForm, page, sessionGuid);

            var routeValuesDictionary = new RouteValueDictionaryBuilder()
                .WithValue("path", path)
                .WithValue("form", form)
                .WithQueryValues(queryParamters)
                .Build();

            return RedirectToAction("Index", "Home", routeValuesDictionary);
        } 

    }
}
