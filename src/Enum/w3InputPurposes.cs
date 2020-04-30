// using System.ComponentModel;

// namespace form_builder.Enum
// {
//     public class EnumExtensions
//     {
//         public static string GetDescription<T>(this T e) where T : IConvertible
//         {
//             if (e is Enum)
//             {
//                 Type type = e.GetType();
//                 Array values = System.Enum.GetValues(type);

//                 foreach (int val in values)
//                 {
//                     if (val == e.ToInt32(CultureInfo.InvariantCulture))
//                     {
//                         var memInfo = type.GetMember(type.GetEnumName(val));
//                         var descriptionAttribute = memInfo[0]
//                             .GetCustomAttributes(typeof(DescriptionAttribute), false)
//                             .FirstOrDefault() as DescriptionAttribute;

//                         if (descriptionAttribute != null)
//                         {
//                             return descriptionAttribute.Description;
//                         }
//                     }
//                 }
//             }
//             return e.ToString();
//         }
//     }


//     public enum w3InputPurposes
//     {
//         [Description("name")]
//         Name,
//         [Description("honorific-prefix")]
//         HonorificPrefix,
//         [Description("given-name")]
//         GivenName,
//         [Description("additional-name")]
//         AdditionalName,
//         [Description("family-name")]
//         FamilyName,
//         [Description("honorific-suffix")]
//         HonorificSuffix,
//         [Description("nickname")]
//         Nickname,
//         [Description("organization-title")]
//         OrganizationTitle,
//         [Description("username")]
//         Username,
//         [Description("new-password")]
//         NewPassword,
//         [Description("current-password")]
//         CurrentPassword,
//         [Description("organization")]
//         Organization,
//         [Description("street-address")]
//         StreetAddress,
//         [Description("address-line1")]
//         AddressLine1,
//         [Description("address-line2")]
//         AddressLine2,
//         [Description("address-line3")]
//         AddressLine3,
//         [Description("given-name")]
//         AddressLine4,
//         [Description("address-level3")]
//         AddressLevel3,
//         [Description("address-level2")]
//         AddressLevel2,
//         [Description("address-level1")]
//         AddressLevel1,
//         [Description("country")]
//         Country,
//         [Description("country-name")]
//         CountryName,
//         [Description("postal-code")]
//         PostalCode,
//         [Description("cc-name")]
//         CCName,
//         [Description("cc-given-name")]
//         CCGivenName,
//         [Description("given-name")]
//         cc-additional-name,
//         [Description("given-name")]
//         cc-family-name,
//         [Description("given-name")]
//         cc-number,
//         [Description("given-name")]
//         cc-exp,
//         [Description("given-name")]
//         cc-exp-month,
//         [Description("given-name")]
//         cc-exp-year,
//         [Description("given-name")]
//         cc-csc,
//         [Description("given-name")]
//         cc-type,
        
//         [Description("given-name")]
//         transaction-currency,
        
//         [Description("transaction-amount")]
//         TransactionAmount,
        
//         [Description("language")]
//         Language,

//         [Description("bday")]
//         Birthday,

//         [Description("bday-day")]
//         BirthdayDay,

//         [Description("bday-month")]
//         BirthdayMonth,
        
//         [Description("bday-year")]
//         BirthdayYear,
        
//         [Description("sex")]
//         Sex,
        
//         [Description("url")]
//         Url,
        
//         [Description("photo")]
//         Photo,
        
//         [Description("tel")]
//         Tel,

//         [Description("tel-country-code")]
//         TelCountryCode,

//         [Description("tel-national")]
//         TelNational,
        
//         [Description("tel-area-code")]
//         TelAreaCode,

//         [Description("tel-local")]
//         TelLocal,

//         [Description("tel-local-prefix")]
//         TelLocalPrefix,

//         [Description("tel-local-suffix")]
//         TelLocalSuffix,

//         [Description("tel-extension")]
//         TelExtension,

//         [Description("email")]
//         Email,

//         [Description("impp")]
//         Impp
// }