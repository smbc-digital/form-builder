using form_builder.Models;
using form_builder.Models.Elements;
using System.Threading.Tasks;

namespace form_builder.Factories.Transform.Lookups
{
    public interface ILookupSchemaTransformFactory
    {
        Task<FormSchema> Transform(FormSchema formSchema);
    }
}
