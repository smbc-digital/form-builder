using form_builder.Enum;

namespace form_builder.Constants
{
    public static class AddAnotherConstants
    {
        public static EElementType[] ValidElements = new[]
        {
            EElementType.Textbox,
            EElementType.Textarea,
            EElementType.DateInput,
            EElementType.DatePicker,
            EElementType.Radio,
            EElementType.Checkbox,
            EElementType.TimeInput
        };
    }
}
