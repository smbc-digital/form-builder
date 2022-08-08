using System.Text.RegularExpressions;

namespace form_builder.Constants
{
    public class StreetConstants
    {
        public const string SELECT_SUFFIX = "-street";
        public const string DESCRIPTION_SUFFIX = "-street-description";
        public static Regex STREET_REGEX = new Regex(@"^[a-zA-Z ]*$");
        public static Regex HOUSE_NUMBER = new Regex(@"^\d+[A-Z]?\s*", RegexOptions.IgnoreCase);
        public static Regex HOUSE_NAME = new Regex(@"^[A-Z]+\s*([A-Z]+\s*)?", RegexOptions.IgnoreCase);
        public static Regex FLAT_AND_NUMBER = new Regex(@"^Flat\s*\d\,*\s+\d+[A-Z]?\s*", RegexOptions.IgnoreCase);
        public static int STREET_MIN_LENGTH = 3;
    }
}