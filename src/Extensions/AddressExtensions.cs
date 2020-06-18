using StockportGovUK.NetStandard.Models.Addresses;

namespace form_builder.Extensions
{
    public static class AddressExtensions
    {
        public static bool IsEmpty(this Address value) => value.IsAutomaticallyFound ?
                string.IsNullOrEmpty(value.PlaceRef) &&
                string.IsNullOrEmpty(value.SelectedAddress) : 
                string.IsNullOrEmpty(value.AddressLine1) &&
                string.IsNullOrEmpty(value.Postcode);
    }
}
