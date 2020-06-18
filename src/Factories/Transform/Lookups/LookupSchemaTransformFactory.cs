using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Transforms.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Factories.Transform.Lookups
{
    public class LookupSchemaTransformFactory : ILookupSchemaTransformFactory
    {
        private readonly ILookupTransformDataProvider _lookupTransformDataProvider;

        public LookupSchemaTransformFactory(ILookupTransformDataProvider lookupTransformDataProvider)
        {
            _lookupTransformDataProvider = lookupTransformDataProvider;
        }

        public async Task<FormSchema> Transform(FormSchema formSchema)
        {
            formSchema.Pages
                .SelectMany(_ => _.ValidatableElements)
                .Where(_ => !string.IsNullOrEmpty(_.Lookup))
                .Select(async element => { return await TransformElement(element); })
                .Select(t => t.Result)
                .ToList();


            return formSchema;
        }

        private async Task<IElement> TransformElement(IElement entry)
        {
            var lookupOptions = await _lookupTransformDataProvider.Get<List<Option>>(entry.Lookup);

            if(!lookupOptions.Any())
            {
                throw new Exception($"LookupSchemaTransformFactory::Build, No lookup options found for question {entry.Properties.QuestionId} with lookup value {entry.Lookup}");
            }

            entry.Properties.Options.AddRange(lookupOptions);

            return entry;
        }
    }
}
