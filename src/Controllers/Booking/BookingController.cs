using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Exceptions;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Services.BookingService;
using form_builder.Services.PageService;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IPageService _pageService;

        public BookingController(IBookingService bookingService,
            ISchemaFactory schemaFactory,
            IPageService pageService)
        {
            _bookingService = bookingService;
            _schemaFactory = schemaFactory;
            _pageService = pageService;
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
            if (string.IsNullOrEmpty(hash) || bookingGuid.Equals(Guid.Empty))
                throw new ApplicationException($"BookingController::CancelBooking, Invalid parameters received. Id: '{bookingGuid}', hash '{hash}' for form '{formName}'");

            try
            {
                var appointment = await _bookingService.ValidateCancellationRequest(formName, bookingGuid, hash);

                return View("AppointmentDetails", new CancelBookingViewModel
                {
                    FormName = appointment.FormName,
                    BaseURL = appointment.BaseURL,
                    StartPageUrl = appointment.StartPageUrl,
                    Id = bookingGuid,
                    BookingDate = appointment.BookingDate,
                    StartTime = appointment.StartTime,
                    EndTime = appointment.EndTime,
                    Cancellable = appointment.Cancellable,
                    Hash = hash,
                    IsFullday = appointment.IsFullday,
                    DisplayBreadCrumbs = false,
                    HideBackButton = true,
                    PageTitle = "Cancel Appointment"
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
            if (string.IsNullOrEmpty(hash) || Guid.Empty.Equals(bookingGuid))
                throw new ApplicationException($"BookingController::CancelBookingPost, Invalid parameters received. Id: '{bookingGuid}', hash '{hash}' for form '{formName}'");

            await _bookingService.Cancel(formName, bookingGuid, hash);

            return RedirectToAction("CancelSuccess", new
            {
                formName
            });
        }

        [HttpGet]
        [Route("{formName}/cannot-cancel-booking")]
        public async Task<IActionResult> CannotCancel(string formName) 
        {
            var formSchema = await _schemaFactory.Build(formName);

            return View("CannotCancel", new FormBuilderViewModel 
            {
                FormName = formSchema.FormName,
                StartPageUrl = formSchema.StartPageUrl,
                PageTitle = "Appointment cannot be cancelled",
                DisplayBreadCrumbs = false,
                HideBackButton = true
            });
        } 

        [HttpGet]
        [Route("{formName}/booking-cancel-success")]
        public async Task<IActionResult> CancelSuccess(string formName) 
        {
            var result = await _pageService.GetCancelBookingSuccessPage(formName);
            var success = new SuccessViewModel
            {
                Reference = result.CaseReference,
                PageContent = result.HtmlContent,
                FormAnswers = result.FormAnswers,
                FormName = result.FormName,
                StartPageUrl = result.StartPageUrl,
                FeedbackPhase = result.FeedbackPhase,
                FeedbackForm = result.FeedbackFormUrl,
                PageTitle = result.PageTitle,
                BannerTitle = result.BannerTitle,
                LeadingParagraph = result.LeadingParagraph,
                DisplayBreadcrumbs = result.DisplayBreadcrumbs,
                Breadcrumbs = result.Breadcrumbs
            };

            return View("../Home/Success", success);
        }
    }
}
