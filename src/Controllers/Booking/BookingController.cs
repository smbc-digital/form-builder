using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Providers.Booking;
using form_builder.Services.BookingService;
using form_builder.Utils.Hash;
using form_builder.ViewModels;
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

            await _bookingService.ProcessMonthRequest(viewModel, form, path);

            var routeValuesDictionary = new RouteValueDictionaryBuilder()
                .WithValue("path", path)
                .WithValue("form", form)
                .WithQueryValues(queryParamters)
                .Build();

            return RedirectToAction("Index", "Home", routeValuesDictionary);
        }

        [HttpGet]
        [Route("booking/{formName}/cancel/{bookingGuid}")]
        public async Task<IActionResult> CancelBooking([FromQuery] string hash, Guid bookingGuid, string formName)
        {
            if (string.IsNullOrEmpty(hash) || Guid.Empty == bookingGuid)
                throw new ApplicationException($"Booking controllers: Invalid cancel model recieved.");

            try
            {
                var appointment = await _bookingService.ValidateCancellationRequest(formName, bookingGuid, hash);

                return View("AppointmentDetails", new CancelBookingViewModel
                {
                    FormName = formName,
                    BaseURL = schema.BaseURL,
                    Id = bookingGuid,
                    BookingDate = appointment.BookingDate,
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    Cancellable = appointment.Cancellable,
                    Hash = hash,
                    IsFullday = appointment.IsFullday,
                    DisplayBreadCrumbs = false,
                    HideBackButton = true,
                    PageTitle = "Cancel Booking"
                });
            }
            catch (BookingCannotBeCancelledException)
            {
                return RedirectToAction("CannotCancel", new
                {
                    formName
                });
            }
        }

        [HttpPost]
        [Route("booking/{formName}/cancel/{bookingGuid}")]
        public async Task<IActionResult> CancelBookingPost([FromQuery] string hash, Guid bookingGuid, string formName)
        {
            //Validate model - hash is not empty, bookign guid is not empty
            if (string.IsNullOrEmpty(hash) || Guid.Empty == bookingGuid)
                throw new ApplicationException($"Booking controllers: Invalid cancel model recieved.");

            var hashedBookingId = _hashUtil.Hash(bookingGuid.ToString());

            //Compare guid and hash
            var hashNotValid = !_hashUtil.Check(bookingGuid.ToString(), hash);
            if (hashNotValid)
                throw new ApplicationException($"Booking controllers: BookingId has been tampered.");

            //Get the formSchema
            var schema = await _schemaFactory.Build(formName);

            var provider = schema.Pages.SelectMany(p => p.Elements)
                .Where(e => e.Type.Equals(EElementType.Booking))
                .FirstOrDefault().Properties.BookingProvider;

            //Call Booking Provider to get appointment info
            //Call .Cancel on booking Provider
            await _bookingProviders.Get(provider).Cancel(bookingGuid);

            //If Successful - show success page
            return RedirectToAction("CancelSuccess", new
            {
                formName
            });
        }

        [HttpGet]
        [Route("booking/{formName}/cannot-cancel-booking")]
        public IActionResult CannotCancel(string formName) => View();

        [HttpGet]
        [Route("booking/{formName}/cancel-success")]
        public IActionResult CancelSuccess(string formName) => View("../Home/Success", new SuccessViewModel { LeadingParagraph = "Thanks for cancelling your appointment", BannerTitle = "Your've successfully cancelled your appointment" });
    }
}
