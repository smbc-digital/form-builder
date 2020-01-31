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
                case "ui-test":
                case "int":
                    return "Int";
                case "qa":
                    return "QA";
                case "stage":
                    return "Staging";
                case "prod":
                    return "Prod";
                default:
                    return "local";
                    //throw new Exception("Unknown environment name");
            }
        }

        public static string ToReturnUrlPrefix(this string value)
        {
            switch (value)
            {
                case "uitest":
                case "local":
                case "prod":
                    return string.Empty;
                case "int":
                case "qa":
                case "stage":
                    return "/formbuilder";
                default:
                    throw new Exception("Unknown environment name");

            }
        }
    }
}
