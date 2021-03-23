using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Providers.Booking;
using form_builder.Services.BookingService;
using form_builder.Utils.Hash;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IPageHelper _pageHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IHashUtil _hashUtil;
        private readonly IEnumerable<IBookingProvider> _bookingProviders;

        public BookingController(IBookingService bookingService,
            ISchemaFactory schemaFactory,
            IPageHelper pageHelper,
            ISessionHelper sessionHelper,
            IHashUtil hashUtil,
            IEnumerable<IBookingProvider> bookingProviders)
        {
            _bookingService = bookingService;
            _schemaFactory = schemaFactory;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _hashUtil = hashUtil;
            _bookingProviders = bookingProviders;
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

        [HttpGet]
        [Route("booking/{form}/cancel/{bookingGuid}")]
        public async Task<IActionResult> CancelBooking([FromQuery] string hash, Guid bookingGuid, string form)
        {
            //validate the model
            if (string.IsNullOrEmpty(hash) || Guid.Empty == bookingGuid)
                throw new ApplicationException($"Booking controllers: Invalid cancel model recieved.");

            //Hash and Salt BookingID
            var hashedBookingId = _hashUtil.Hash(bookingGuid.ToString());

            //Compare hash value annd go to error if don't match
            var hashNotValid = !_hashUtil.Check(bookingGuid.ToString(), hash);
            if (hashNotValid)
                throw new ApplicationException($"Booking controllers: BookingId has been tampered.");

            var schema = await _schemaFactory.Build(form);

            var provider = schema.Pages.SelectMany(p => p.Elements)
                .Where(e => e.Type.Equals(EElementType.Booking))
                .FirstOrDefault().Properties.BookingProvider;

            //Call Booking Provider to get appointment info
            var bookingProvider = _bookingProviders.Get(provider);
            var appointment = await bookingProvider.GetAppointment(bookingGuid);

            //Get a response back
            //Handle the response can it cancel the booking
            //Can cancel then render page

            return View();
        }
    }
}
