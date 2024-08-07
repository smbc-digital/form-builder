using System.Text.RegularExpressions;

namespace form_builder.Constants
{
    public class StreetConstants
    {
        public const string SELECT_SUFFIX = "-street";
        public const string DESCRIPTION_SUFFIX = "-street-description";
        public static Regex STREET_REGEX = new Regex(@"^[a-zA-Z ]*$");
        public static int STREET_MIN_LENGTH = 3;
        public const string STREET_MANUAL_TEXT = "I cannot find the street in the list";
    }
}