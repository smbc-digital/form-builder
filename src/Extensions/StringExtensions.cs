using System;
using System.IO;
using form_builder.Constants;

namespace form_builder.Extensions
{
    public static class StringExtensions
    {
        public static string ToS3EnvPrefix(this string value)
        {
            switch (value)
            {
                case "local":
                    return "local";
                case "uitest":
                case "int":
                    return "Int";
                case "qa":
                    return "QA";
                case "stage":
                    return "Stage";
                case "prod":
                    return "Prod";
                default:
                    throw new Exception("Unknown environment name");
            }
        }

        public static string ToReturnUrlPrefix(this string value)
        {
            switch (value)
            {
                case "uitest":
                case "local":
                case "prod":
                case "int":
                case "qa":
                case "stage":
                    return string.Empty;
                default:
                    throw new Exception("Unknown environment name");
            }
        }

        public static int ToReadableMaxFileSize(this int value)
        {
            var megaByteValue = value / SystemConstants.OneMBInBinaryBytes;
            return Convert.ToInt32(megaByteValue);
        }

        public static string ToMaxSpecifiedStringLengthForFileName(this string value, int length)
        {
            if (value.Length <= length)
                return value;

            var extension = Path.GetExtension(value);

            return $"{value.Substring(0, length - extension.Length)}{extension}";
        }

        public static string ToBookingRequestedMonthUrl(this string form, string page) => $"/booking/{form}/{page}/month";
    }
}
