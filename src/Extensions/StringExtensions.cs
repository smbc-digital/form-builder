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
                case "int":
                    return "Int";
                case "qa":
                    return "QA";
                case "stage":
                    return "Staging";
                case "prod":
                    return "Prod";
                default:
                    throw new Exception("Unknown environment name");

            }
        }
    }
}
