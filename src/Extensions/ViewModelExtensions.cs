using form_builder.Constants;
using System.Collections.Generic;

namespace form_builder.Extensions
{
    public static class ViewModelExtensions
    {
        public static bool IsAutomatic(this Dictionary<string, dynamic> dictionary)
            => dictionary.ContainsKey(LookUpConstants.SubPathViewModelKey)
            && dictionary[LookUpConstants.SubPathViewModelKey] == LookUpConstants.Automatic;

        public static bool IsManual(this Dictionary<string, dynamic> dictionary)
            => dictionary.ContainsKey(LookUpConstants.SubPathViewModelKey)
            && dictionary[LookUpConstants.SubPathViewModelKey] == LookUpConstants.Manual;

        public static bool IsInitial(this Dictionary<string, dynamic> dictionary)
            => !(dictionary.IsAutomatic() || dictionary.IsManual());
    }
}
