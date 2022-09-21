using form_builder.Models;

namespace form_builder.Validators.IntegrityChecks
{
    public interface IFormSchemaIntegrityValidator
    {
        Task Validate(FormSchema schema);
    }
}