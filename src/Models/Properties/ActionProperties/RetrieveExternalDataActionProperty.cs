using System.Collections.Generic;

namespace form_builder.Models.Properties.ActionProperties
{
    public partial class BaseActionProperty
    {
        public string TargetQuestionId { get; set; }

        public List<PageActionSlug> PageActionSlugs { get; set; }

    }

    public class PageActionSlug
    {
        public string Environment { get; set; }

        public string URL { get; set; }

        public string AuthToken { get; set; } = string.Empty;
    }
}