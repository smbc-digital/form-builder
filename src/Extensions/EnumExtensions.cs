using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
                    return "form-json-";
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

        public static string GetEnumDescription<T>(this T e) where T : IConvertible
        {
            Type type = e.GetType();
            Array values = System.Enum.GetValues(type);

            foreach (int val in values)
            {
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    var memInfo = type.GetMember(type.GetEnumName(val));
                    var descriptionAttribute = memInfo[0]
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;

                    if (descriptionAttribute != null)
                    {
                        return descriptionAttribute.Description;
                    }
                }
            }
            return e.ToString();
        }
    }
}
