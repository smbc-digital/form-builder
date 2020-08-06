using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Factories.Transform.ReusableElements
{
    public interface IReusableElementSchemaTransformFactory
    {
        Task<FormSchema> Transform(FormSchema formSchema);
    }
}