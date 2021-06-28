using System.Collections.Generic;
using form_builder.Constants;

namespace form_builder.Extensions
{
    public static class ViewModelExtensions
    {
        public static bool IsAutomatic(this Dictionary<string, dynamic> dictionary)
            => dictionary.ContainsKey(LookUpConstants.SubPathViewModelKey)
            && dictionary[LookUpConstants.SubPathViewModelKey].Equals(LookUpConstants.Automatic);

        public static bool IsManual(this Dictionary<string, dynamic> dictionary)
            => dictionary.ContainsKey(LookUpConstants.SubPathViewModelKey)
            && dictionary[LookUpConstants.SubPathViewModelKey].Equals(LookUpConstants.Manual);

        public static bool IsInitial(this Dictionary<string, dynamic> dictionary)
            => !(dictionary.IsAutomatic() || dictionary.IsManual());

        public static bool IsCheckYourBooking(this Dictionary<string, dynamic> dictionary)
            => dictionary.ContainsKey(LookUpConstants.SubPathViewModelKey)
            && dictionary[LookUpConstants.SubPathViewModelKey].Equals(BookingConstants.CHECK_YOUR_BOOKING);
    }
}
