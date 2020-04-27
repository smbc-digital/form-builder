using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.TransformDataProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Factories.Transform
{
    public interface ISchemaTransformFactory
    {
        Task<T> Build<T>(IElement entry);
    }

    public class LookupSchemaFactory : ISchemaTransformFactory
    {
        private readonly ITransformDataProvider _transformDataProvider;

        public LookupSchemaFactory(ITransformDataProvider transformDataProvider)
        {
            _transformDataProvider = transformDataProvider;
        }

        public async Task<T> Build<T>(IElement entry)
        {
            var lookupOptions = await _transformDataProvider.Get<List<Option>>(entry.Lookup);

            if(lookupOptions == null)
                throw new Exception($"LookupSchemaFactory::Build, No lookup options found for question {entry.Properties.QuestionId} with lookup value {entry.Lookup}");

            entry.Properties.Options.AddRange(lookupOptions);

            return (T)entry;
        }
    }
}
