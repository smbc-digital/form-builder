using System;

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
    }
}
