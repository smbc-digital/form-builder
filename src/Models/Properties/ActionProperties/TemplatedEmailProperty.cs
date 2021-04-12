using System.Collections.Generic;

namespace form_builder.Models.Properties.ActionProperties
{
    public partial class BaseActionProperty
    {
        public string TemplateId { get; set; }
        public string EmailTemplateProvider { get; set; }
        public List<string> Personlisation { get; set; }
    }
}
