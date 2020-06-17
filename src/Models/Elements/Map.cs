using form_builder.Enum;

namespace form_builder.Models.Elements
{
    public class Map : Element
    {
        public string MainJSFile => $"{Properties.Source}/main-latest.js";
        public string VendorJSFIle => $"{Properties.Source}/vendor-latest.js";
        public Map()
        {
            Type = EElementType.Map;
        }
    }
}
