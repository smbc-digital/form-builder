using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators
{
    public interface IFormSchemaIntegrityValidator
    {
        Task Validate(FormSchema schema);
    }
}