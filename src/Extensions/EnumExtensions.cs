using System;
using form_builder.Enum;
using form_builder.Constants;

namespace form_builder.Extensions
{
    public static class EnumExtensions
    {
        public static string ToESchemaTypePrefix(this ESchemaType value)
        {
            switch (value)
            {
                case ESchemaType.FormJson:
                    return $"form-json-";
                case ESchemaType.PaymentConfiguration:
                    return "paymentconfig-";
                default:
                    throw new Exception("Unknown schema type");
            }
        }

        public static string ToContentType(this EDocumentType value)
        {
            switch (value)
            {
                case EDocumentType.Txt:
                    return "text/plain";
                default:
                    throw new Exception("Unknown document type");
            }
        }

        public static string ToReadableTextForAnlayticsEvent(this EAnalyticsEventType value)
            => value switch
            {
                EAnalyticsEventType.Finish => AnalyticsConstants.FINISH_EVENT_LABEL_NAME,
                _ => throw new ArgumentOutOfRangeException($"Failed to convert EAnalyticsEventType: {value} to readble text")
            };
    }
}