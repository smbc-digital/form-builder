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
            var addressFields = address.Split(',');
            var isHouseNumber = int.TryParse(addressFields[0].Split(' ')[0], out _);
            var addressDetails = new AddressDetail
            {
                Town = addressFields[^2].Trim(),
                Postcode = addressFields[^1].Trim()
            };

            if (isHouseNumber)
            {
                addressDetails.HouseNo = addressFields[0].Split(' ')[0].Trim();
                addressDetails.Street = string.Join(" ", addressFields[0].Split(' ')[1..]);
            }
            else
            {
                addressDetails.Street = addressFields[0].Trim();

            }

            return addressDetails;
        }

        public static string RemoveTagParsingFromQuestionId(this string questionId)
        {
            if (string.IsNullOrEmpty(questionId))
                return string.Empty;

            return questionId.Replace("{{QUESTION:", String.Empty).Replace("}}", String.Empty);   
        }
    }
}
