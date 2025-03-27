using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace form_builder.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EDocumentType
    {
        Unknown,
        Txt,
        Html,
        Pdf,
        Word
    }
}