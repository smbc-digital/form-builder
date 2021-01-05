using StockportGovUK.NetStandard.Models.Addresses;
using System.Globalization;

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
    }
}
