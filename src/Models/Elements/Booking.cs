using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models.Booking;
using form_builder.Models.Time;
using form_builder.Utils.Extensions;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Response;

namespace form_builder.Models.Elements;

public class Booking : Element
{
    public Booking() => Type = EElementType.Booking;
    public List<CalendarDay> Calendar { get; set; } = new List<CalendarDay>();
    public List<TimeAvailability> Times { get; set; } = new List<TimeAvailability>();
    public List<AvailabilityDayResponse> Appointments { get; set; } = new List<AvailabilityDayResponse>();
    [JsonIgnore]
    public string FormattedDateForCheckYourBooking => DateTime.Parse(Properties.Value).ToFullDateFormat();
    [JsonIgnore]
    public string FormattedTimeForCheckYourBooking => SelectedBooking.IsFullDayAppointment ? $"between {AppointmentStartTime.ToTimeFormat()} and {AppointmentEndTime.ToTimeFormat()}" : $"{AppointmentStartTime.ToTimeFormat()} to {AppointmentEndTime.ToTimeFormat()}";
    public AvailabilityDayResponse SelectedBooking;
    public bool IsAppointmentTypeFullDay { get; set; }
    public DateTime AppointmentStartTime { get; set; }
    public DateTime AppointmentEndTime { get; set; }
    public DateTime CurrentSelectedMonth { get; set; }
    public DateTime FirstAvailableMonth { get; set; }
    public string AppointmentTypeFullDayIAG => $"You can select a date but you cannot select a time. We’ll be with you between {AppointmentStartTime.ToTimeFormat()} and {AppointmentEndTime.ToTimeFormat()}.";
    public bool DisplayNextAvailableAppointmentIAG => FirstAvailableMonth.Date > DateTime.Now.Date && CurrentSelectedMonth.Month.Equals(FirstAvailableMonth.Month) && CurrentSelectedMonth.Year.Equals(FirstAvailableMonth.Year);
    public string InsetText => SetInsetText();
    public bool DisplayInsetText => InsetText.Length > 0;
    public string CurrentSelectedMonthText => $"{CurrentSelectedMonth:MMMMM yyyy}";
    [JsonIgnore]
    public DateTime NextSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(1);
    [JsonIgnore]
    public DateTime PreviousSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(-1);
    [JsonIgnore]
    public bool DisplayNextMonthArrow => NextSelectableMonth <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(Properties.SearchPeriod);
    [JsonIgnore]
    public bool DisplayPreviousMonthArrow => new DateTime(CurrentSelectedMonth.Date.Year, CurrentSelectedMonth.Date.Month, 1) > FirstAvailableMonth;
    public override string GetLabelText(string pageTitle) => $"{(string.IsNullOrEmpty(Properties.SummaryLabel) ? "Booking" : Properties.SummaryLabel)}{GetIsOptionalLabelText()}";
    public string FormName { get; set; }
    public string ReservedBookingId { get; set; }
    public string ReservedAppointmentId { get; set; }
    public string ReservedBookingDate { get; set; }
    public string ReservedBookingStartTime { get; set; }
    public string ReservedBookingEndTime { get; set; }
    public string StartAppointmentTime { get; set; }
    public string EndAppointmentTime { get; set; }
    public string MonthSelectionPostUrl { get; set; }
    public string DateQuestionId => $"{Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}";
    public string StartTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}";
    public string EndTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}";
    public string ReservedDateQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_DATE}";
    public string ReservedAppointmentIdQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_APPOINTMENT_ID}";
    public string ReservedStartTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}";
    public string ReservedEndTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_END_TIME}";
    public string ReservedIdQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_ID}";
    public string AppointmentLocation => $"{Properties.QuestionId}-{BookingConstants.APPOINTMENT_LOCATION}";
    public string NoTimeForBookingType => Properties.NoAvailableTimeForBookingType;

    public override Task<string> RenderAsync(IViewRender viewRender,
        IElementHelper elementHelper,
        string cacheKey,
        Dictionary<string, dynamic> viewModel,
        Page page,
        FormSchema formSchema,
        IWebHostEnvironment environment,
        FormAnswers formAnswers,
        List<object> results = null)
    {
        viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

        ConfigureBookingInformation(results);
        FormName = formSchema.FormName;

        Properties.Value = elementHelper.CurrentValue(DateQuestionId, viewModel, formAnswers);

        if (string.IsNullOrEmpty(Properties.Value) && !DisplayPreviousMonthArrow)
            Properties.Value = Appointments.OrderBy(_ => _.Date).FirstOrDefault()?.Date.ToString();

        ReservedBookingId = elementHelper.CurrentValue(ReservedIdQuestionId, viewModel, formAnswers);
        ReservedBookingDate = elementHelper.CurrentValue(ReservedDateQuestionId, viewModel, formAnswers);
        ReservedAppointmentId = elementHelper.CurrentValue(ReservedAppointmentIdQuestionId, viewModel, formAnswers);
        ReservedBookingStartTime = IsAppointmentTypeFullDay ? AppointmentStartTime.ToString() : elementHelper.CurrentValue(ReservedStartTimeQuestionId, viewModel, formAnswers);
        ReservedBookingEndTime = IsAppointmentTypeFullDay ? AppointmentEndTime.ToString() : elementHelper.CurrentValue(ReservedEndTimeQuestionId, viewModel, formAnswers);
        StartAppointmentTime = IsAppointmentTypeFullDay ? AppointmentStartTime.ToString() : elementHelper.CurrentValue(StartTimeQuestionId, viewModel, formAnswers);
        EndAppointmentTime = IsAppointmentTypeFullDay ? AppointmentEndTime.ToString() : elementHelper.CurrentValue(EndTimeQuestionId, viewModel, formAnswers);

        switch (subPath as string)
        {
            case BookingConstants.CHECK_YOUR_BOOKING:
                SelectedBooking = GetSelectedAppointment();
                return viewRender.RenderAsync("CheckYourBooking", this);
            default:
                CreateCalendar();
                CreateTimeAvailability();
                MonthSelectionPostUrl = formSchema.BaseURL.ToBookingRequestedMonthUrl(page.PageSlug);
                return viewRender.RenderAsync(Type.ToString(), this);
        }
    }

    private void ConfigureBookingInformation(List<object> results)
    {
        var bookingInformation = (BookingInformation)results.First();

        Appointments = bookingInformation.Appointments;
        CurrentSelectedMonth = bookingInformation.CurrentSearchedMonth;
        FirstAvailableMonth = bookingInformation.FirstAvailableMonth;
        IsAppointmentTypeFullDay = bookingInformation.IsFullDayAppointment;

        if (bookingInformation.IsFullDayAppointment)
        {
            AppointmentStartTime = bookingInformation.AppointmentStartTime;
            AppointmentEndTime = bookingInformation.AppointmentEndTime;
        }
    }

    private void CreateCalendar()
    {
        var dates = new List<CalendarDay>();
        var daysInMonth = DateTime.DaysInMonth(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month);
        var daysToAdd = new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).PreviousDaysInMonth();

        for (int i = 0; i < daysToAdd; i++)
        {
            dates.Add(new CalendarDay { IsDisabled = true, IsNotWithinCurrentSelectedMonth = true });
        }

        for (int i = 0; i < daysInMonth; i++)
        {
            var day = new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, i + 1);
            var containsDay = Appointments.FirstOrDefault(_ => _.Date.Equals(day));
            dates.Add(new CalendarDay { Date = day, IsDisabled = containsDay is null, Checked = containsDay is not null && Properties.Value.Equals(day.Date.ToString()) });
        }

        for (int i = dates.Count + 1; i < 43; i++)
        {
            dates.Add(new CalendarDay { IsDisabled = true, IsNotWithinCurrentSelectedMonth = true });
        }
        Calendar = dates;
    }

    private void CreateTimeAvailability()
    {
        if (IsAppointmentTypeFullDay)
            return;

        var days = Appointments.Where(_ => _.HasAvailableAppointment);

        if (!days.Any())
            return;

        Times = days.Select((day) =>
        {
            var morningAppointments = day.AppointmentTimes.Where(_ => _.StartTime.Hours < 12).ToList();
            var afternoonAppointments = day.AppointmentTimes.Where(_ => _.StartTime.Hours >= 12).ToList();
            return new TimeAvailability
            {
                Date = day.Date,
                MorningAppointments = new TimePeriod
                {
                    Appointments = morningAppointments,
                    TimeQuestionId = StartTimeQuestionId,
                    TimeOfDay = ETimePeriod.Morning,
                    Date = day.Date,
                    CurrentValue = Properties.Value.Equals(day.Date.ToString()) && !string.IsNullOrEmpty(StartAppointmentTime) ? DateTime.Parse(StartAppointmentTime).ToString() : string.Empty,
                    BookingType = NoTimeForBookingType
                },
                AfternoonAppointments = new TimePeriod
                {
                    Appointments = afternoonAppointments,
                    TimeQuestionId = StartTimeQuestionId,
                    TimeOfDay = ETimePeriod.Afternoon,
                    Date = day.Date,
                    CurrentValue = Properties.Value.Equals(day.Date.ToString()) && !string.IsNullOrEmpty(StartAppointmentTime) ? DateTime.Parse(StartAppointmentTime).ToString() : string.Empty,
                    BookingType = NoTimeForBookingType
                },
                TimePeriodCurrentlySelected = Properties.Value.Equals(day.Date.ToString()) && !string.IsNullOrEmpty(StartAppointmentTime) && Properties.Value.Equals(DateTime.Parse(StartAppointmentTime).Date.ToString()) ? DateTime.Parse(StartAppointmentTime).Hour >= 12 ? ETimePeriod.Afternoon : ETimePeriod.Morning : morningAppointments.Any() ? ETimePeriod.Morning : ETimePeriod.Afternoon,
                TimeQuestionId = StartTimeQuestionId,
                DateQuestionId = DateQuestionId
            };
        }).ToList();
    }

    private AvailabilityDayResponse GetSelectedAppointment()
    {
        var selectedAppointment = Appointments.FirstOrDefault(_ => _.Date.ToString().Equals(Properties.Value));

        if (selectedAppointment is null)
            throw new ApplicationException("Booking::GetSelectedAppointment, Unable to find selected appointment while attempting to generate check your booking view");

        AppointmentStartTime = DateTime.Parse(StartAppointmentTime);
        AppointmentEndTime = DateTime.Parse(EndAppointmentTime);

        return selectedAppointment;
    }

    private string SetInsetText()
    {
        if (IsAppointmentTypeFullDay && DisplayNextAvailableAppointmentIAG)
            return $"{Properties.NextAvailableIAG} {AppointmentTypeFullDayIAG}";

        if (IsAppointmentTypeFullDay && !DisplayNextAvailableAppointmentIAG)
            return AppointmentTypeFullDayIAG;

        if (!IsAppointmentTypeFullDay && DisplayNextAvailableAppointmentIAG)
            return Properties.NextAvailableIAG;

        return string.Empty;
    }
}