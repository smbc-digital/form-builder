using form_builder.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace form_builder.ModelBinders.Providers
{
    public class CustomFormFileModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType.Equals(typeof(CustomFormFile)))
                return new BinderTypeModelBinder(typeof(CustomFormFileModelBinder));

            return typeof(IEnumerable<CustomFormFile>).IsAssignableFrom(context.Metadata.ModelType) ? new BinderTypeModelBinder(typeof(CustomFormFileModelBinder)) : null;
        }
    }
}