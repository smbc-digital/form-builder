using form_builder.Models.Elements;

namespace form_builder.Providers.Transforms.ReusableElements
{
    public interface IReusableElementTransformDataProvider
    {
        Task<IElement> Get(string schemaName);
    }
}