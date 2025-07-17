using form_builder.Enum;

namespace form_builder.Models
{
    public class IncomingValue
    {
        public string QuestionId { get; set; }
        public string Name { get; set; }
        public bool Optional { get; set; }
        public EHttpActionType HttpActionType { get; set; }
        public bool Base64Encoded { get; set; }
        public string GetExampleValue() => Base64Encoded ? $"{Name}=YourBase64EncodedValueHere" : $"{Name}=YourValueHere";
    }
}