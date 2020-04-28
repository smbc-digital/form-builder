using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.TransformDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Factories.Transform
{
    public interface ISchemaTransformFactory
    {
        Task<T> Transform<T>(IElement entry);
    }

    public class LookupSchemaTransformFactory : ISchemaTransformFactory
    {
        private readonly ITransformDataProvider _transformDataProvider;

        public LookupSchemaTransformFactory(ITransformDataProvider transformDataProvider)
        {
            _transformDataProvider = transformDataProvider;
        }

        public async Task<T> Transform<T>(IElement entry)
        {
            var lookupOptions = await _transformDataProvider.Get<List<Option>>(entry.Lookup);

            if(!lookupOptions.Any())
                throw new Exception($"LookupSchemaFactory::Build, No lookup options found for question {entry.Properties.QuestionId} with lookup value {entry.Lookup}");

            entry.Properties.Options.AddRange(lookupOptions);

            return (T)entry;
        }
    }
}
