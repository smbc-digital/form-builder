using form_builder.Models;

namespace form_builder.Services.MappingService.Entities
{
    public class MappingEntity
    {
        public object Data { get; set; }

        public FormSchema BaseForm { get; set; }

        public FormAnswers FormAnswers { get; set; }
    }
}
