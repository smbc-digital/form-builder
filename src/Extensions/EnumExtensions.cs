using form_builder.Enum;

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
                case EDocumentType.Pdf:
                    return "application/pdf";   
                default:
                    throw new Exception("Unknown document type");
            }
        }
    }
}