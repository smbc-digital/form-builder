namespace form_builder.Models.Properties.ActionProperties
{
    public partial class BaseActionProperty
    {
        public string URL { get; set; }

        public string TargetQuestionId { get; set; }

        public string AuthToken { get; set; } = string.Empty;
    }
}
