using form_builder.Models;

namespace form_builder.Factories.Transform.Lookups
{
    public interface ILookupSchemaTransformFactory
    {
        FormSchema Transform(FormSchema formSchema);
    }
}