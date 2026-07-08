namespace form_builder.Controllers.Booking;

public class BookingController(IBookingService bookingService,
    ISchemaFactory schemaFactory,
    IPageService pageService)
    : Controller
{

    [HttpPost]
    [Route("booking/{form}/{path}/month")]
    public async Task<IActionResult> Index(
        string form,
        string path,
        Dictionary<string, string[]> formData)
    {
        var viewModel = formData.ToNormaliseDictionary(string.Empty);
        var queryParamters = Request.Query;

        await bookingService.ProcessMonthRequest(viewModel, form, path);

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
            var appointment = await bookingService.ValidateCancellationRequest(formName, bookingGuid, hash);

            return View("AppointmentDetails", new CancelBookingViewModel
            {
                FormName = appointment.FormName,
                BaseURL = appointment.BaseURL,
                StartPageUrl = appointment.StartPageUrl,
                Id = bookingGuid,
                BookingDate = appointment.BookingDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
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

        await bookingService.Cancel(formName, bookingGuid, hash);

        return RedirectToAction("CancelSuccess", new
        {
            formName
        });
    }

    [HttpGet]
    [Route("{formName}/cannot-cancel-booking")]
    public async Task<IActionResult> CannotCancel(string formName)
    {
        var formSchema = await schemaFactory.Build(formName);

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
        var result = await pageService.GetCancelBookingSuccessPage(formName);
        var success = new SuccessViewModel
        {
            Reference = result.CaseReference,
            PageContent = result.HtmlContent,
            FormAnswers = result.FormAnswers,
            FormName = result.FormName,
            StartPageUrl = result.StartPageUrl,
            Embeddable = result.Embeddable,
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