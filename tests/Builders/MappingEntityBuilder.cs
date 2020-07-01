using form_builder.Models;
using form_builder.Services.MappingService.Entities;

namespace form_builder_tests.Builders
{
    public class MappingEntityBuilder
    {
        private FormSchema _baseForm = new FormSchema();
        private FormAnswers _formAnswers = new FormAnswers();
        private object _data = new object();

        public MappingEntity Build()
        {
            return new MappingEntity
            {
                BaseForm = _baseForm,
                FormAnswers = _formAnswers,
                Data = _data
            };
        }

        public MappingEntityBuilder WithBaseForm(FormSchema baseForm)
        {
            _baseForm = baseForm;
            return this;
        }

        public MappingEntityBuilder WithFormAnswers(FormAnswers formAnswers)
        {
            _formAnswers = formAnswers;
            return this;
        }

        public MappingEntityBuilder WithData(object data)
        {
            _data = data;
            return this;
        }
    }
}
