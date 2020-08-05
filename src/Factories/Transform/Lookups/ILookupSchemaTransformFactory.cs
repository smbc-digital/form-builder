using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Factories.Transform.Lookups
{
    public interface ILookupSchemaTransformFactory
    {
        Task<FormSchema> Transform(FormSchema formSchema);
    }
}
