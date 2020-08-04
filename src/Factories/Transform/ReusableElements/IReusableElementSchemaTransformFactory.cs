using form_builder.Models;
using System.Threading.Tasks;

namespace form_builder.Factories.Transform.ReusableElements
{
    public interface IReusableElementSchemaTransformFactory
    {
        Task<FormSchema> Transform(FormSchema formSchema);
    }
}