using form_builder.Enum;
using System;

namespace form_builder.Extensions
{
    public static class EnumExtensions
    {
        public static string ToESchemaTypePrefix(this ESchemaType value)
        {
            switch (value)
            {
                case ESchemaType.FormJson:
                    return "form-";
                case ESchemaType.PaymentConfiguration:
                    return "paymentconfig-";
                default:
                    throw new Exception("Unknown schema type");

            }
        }
    }
}
