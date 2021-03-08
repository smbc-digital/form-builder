using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Validators
{
    public interface IFormSchemaIntegrityValidator
    {
        Task<bool> Validate(FormSchema schema, string form, string path);
    }
}