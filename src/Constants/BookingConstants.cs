namespace form_builder.Constants
{
    public class BookingConstants
    {
        public const string FAKE_PROVIDER = "Fake";
        public const string APPOINTMENT_TYPE_SEARCH_RESULTS = "appointment-search-results";
        public const string APPOINTMENT_ID = "appointment-id";
        public const string APPOINTMENT_DATE = "appointment-date";
        public const string APPOINTMENT_START_TIME = "appointment-start-time";
        public const string APPOINTMENT_END_TIME = "appointment-end-time";
        public const string CHECK_YOUR_BOOKING = "check-your-booking";
        public const string RESERVED_BOOKING_DATE = "reserved-date";
        public const string RESERVED_BOOKING_START_TIME = "reserved-start-time";
        public const string RESERVED_BOOKING_END_TIME = "reserved-end-time";
        public const string RESERVED_BOOKING_ID = "reserved-booking-id";
        public const string RESERVED_APPOINTMENT_ID = "reserved-appointment-id";
        public const string BOOKING_MONTH_REQUEST = "month-request";
        public const string NO_APPOINTMENT_AVAILABLE = "no-appointment-available";
        public const string APPOINTMENT_TIME_OF_DAY_SUFFIX = "-time-of-day";
        public const string APPOINTMENT_FULL_TIME_OF_DAY_SUFFIX = "-full-time-period";
        public const string APPOINTMENT_TIME_OF_DAY_MORNING = "Morning";
        public const string APPOINTMENT_TIME_OF_DAY_AFTERNOON = "Afternoon";
        public const string APPOINTMENT_LOCATION = "appointment-location";

        public const string INTEGRITY_FAILURE_MESSAGE_NOAPPOINTMENTPAGE = "Booking Form Check: Contains booking element, but is missing required page with slug \"no-appointment-available\".";
        public const string INTEGRITY_FAILURE_MESSAGE_REQUIREDFIELDS = "Booking Form Check: Required Field missing.";
        public const string INTEGRITY_FAILURE_MESSAGE_DUPLICATEPROVIDER = "Booking Form Check: Contains different booking provider. Only one provider allows on for form.";
        public const string INTEGRITY_FAILURE_MESSAGE_APPOINTMENTIDKEY_SOURCE_NOTONPREVIOUSPAGE = "Booking Form Check: Source For AppointmentIdKey is not on previous page.";
        public const string INTEGRITY_FAILURE_MESSAGE_APPOINTMENTIDKEY_DOESNOTEXIST = "Booking Form Check: AppointmentIdKey does not exist... check corresponding QuestionId on your previous page.";
        public const string INTEGRITY_FAILURE_MESSAGE_APPOINTMENTIDKEY_SOURCE_VALUENOTGUID = "Booking Form Check: AppointmentIdKey Value is not a Guid. Check value for option.";
    }
}