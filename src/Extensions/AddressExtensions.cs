using System;
using System.Globalization;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Civica.Pay.Request;

namespace form_builder.Extensions
{
    public static class AddressExtensions
    {
        public static bool IsEmpty(this Address value) => value.IsAutomaticallyFound ?
                string.IsNullOrEmpty(value.PlaceRef) &&
                string.IsNullOrEmpty(value.SelectedAddress) :
                string.IsNullOrEmpty(value.AddressLine1) &&
                string.IsNullOrEmpty(value.Postcode);

        public static string ConvertAddressToTitleCase(this string address)
        {
            var splitAddress = address.Split(',');
            var result = string.Empty;
            var formatCulture = new CultureInfo("en-GB", false).TextInfo;

            for (int i = 0; i < splitAddress.Length - 1; i++)
            {
                result += formatCulture.ToTitleCase(splitAddress[i].ToLower()) + ",";
            }

            result += splitAddress[splitAddress.Length - 1].ToUpper();
            return result;
        }

        public static AddressDetail ConvertStringToObject(this string address)
        {
            var addressDetails = address.Split(',');

            return new AddressDetail()
            {
                HouseNo = addressDetails[0].Split(' ')[0].Trim(),
                Street = $"{addressDetails[0].Split(' ')[1].Trim()} {addressDetails[0].Split(' ')[2].Trim()}",
                Area = addressDetails[1].Trim(),
                Town = addressDetails[2].Trim(),
                County = addressDetails[3].Trim(),
                Postcode = addressDetails[4].Trim()
            };
        }

        public static string RemoveTagParsingFromQuestionId(this string questionId) =>
             questionId.Replace("{{QUESTION:", String.Empty).Replace("}}", String.Empty);   
    }
}
