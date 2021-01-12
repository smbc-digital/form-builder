using System.ComponentModel;

namespace form_builder.Enum
{
    public enum w3InputPurposes
    {
        [Description("name")]
        Name,

        [Description("honorific-prefix")]
        HonorificPrefix,

        [Description("given-name")]
        GivenName,

        [Description("additional-name")]
        AdditionalName,

        [Description("family-name")]
        FamilyName,

        [Description("honorific-suffix")]
        HonorificSuffix,

        [Description("nickname")]
        Nickname,

        [Description("organization-title")]
        OrganizationTitle,

        [Description("username")]
        Username,

        [Description("new-password")]
        NewPassword,

        [Description("current-password")]
        CurrentPassword,

        [Description("organization")]
        Organization,

        [Description("street-address")]
        StreetAddress,

        [Description("address-line1")]
        AddressLine1,

        [Description("address-line2")]
        AddressLine2,

        [Description("address-line3")]
        AddressLine3,

        [Description("given-name")]
        AddressLine4,

        [Description("address-level3")]
        AddressLevel3,

        [Description("address-level2")]
        AddressLevel2,

        [Description("address-level1")]
        AddressLevel1,

        [Description("country")]
        Country,

        [Description("country-name")]
        CountryName,

        [Description("postal-code")]
        PostalCode,

        [Description("cc-name")]
        CCName,

        [Description("cc-given-name")]
        CCGivenName,

        [Description("cc-additional-name,")]
        CCAdditionalName,

        [Description("cc-family-name")]
        CCFamilyName,

        [Description("cc-number,")]
        CCNumber,

        [Description("cc-exp")]
        CCExp,

        [Description("cc-exp-month")]
        CCExpMonth,

        [Description("cc-exp-year")]
        CCExpYear,

        [Description("cc-csc")]
        CCCsc,

        [Description("cc-type")]
        CCType,

        [Description("transaction-currency")]
        TransactionCurrency,

        [Description("transaction-amount")]
        TransactionAmount,

        [Description("language")]
        Language,

        [Description("bday")]
        Birthday,

        [Description("bday-day")]
        BirthdayDay,

        [Description("bday-month")]
        BirthdayMonth,

        [Description("bday-year")]
        BirthdayYear,

        [Description("sex")]
        Sex,

        [Description("url")]
        Url,

        [Description("photo")]
        Photo,

        [Description("tel")]
        Tel,

        [Description("tel-country-code")]
        TelCountryCode,

        [Description("tel-national")]
        TelNational,

        [Description("tel-area-code")]
        TelAreaCode,

        [Description("tel-local")]
        TelLocal,

        [Description("tel-local-prefix")]
        TelLocalPrefix,

        [Description("tel-local-suffix")]
        TelLocalSuffix,

        [Description("tel-extension")]
        TelExtension,

        [Description("email")]
        Email,

        [Description("impp")]
        Impp
    }
}