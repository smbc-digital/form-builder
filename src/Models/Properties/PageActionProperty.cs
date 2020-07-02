namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        public string URL { get; set; }

        public string TargetQuestionId { get; set; }

        public string AuthToken { get; set; } = string.Empty;
    }
}
