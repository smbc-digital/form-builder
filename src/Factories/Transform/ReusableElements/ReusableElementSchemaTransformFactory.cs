using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Transforms.ReusableElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Factories.Transform.ReusableElements
{
    public class ReusableElementSchemaTransformFactory : IReusableElementSchemaTransformFactory
    {
        private readonly IReusableElementTransformDataProvider _reusableElementTransformDataProvider;

        public ReusableElementSchemaTransformFactory(IReusableElementTransformDataProvider reusableElementTransformDataProvider)
        {
            _reusableElementTransformDataProvider = reusableElementTransformDataProvider;
        }

        public async Task<FormSchema> Transform(FormSchema formSchema)
        {
            // I haven't found a reasonable way to both calculate then changes required and apply them concurrently, 
            // this might be a possible solution; https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8
            // however updating as you enumerate is problematic 
            // so this is split into to parts 
            // The first part traverses the lists builds and returns a list of reusable element references that require substiuting into the schema (async)
            // The second part applies the substitutions (sync)
            // TODO: Check the performance of this with larger schemas
            var elementSubstitutions = await GetReusableElementSubstitutions(formSchema);
            return ApplyReusableElementSubstitutions(formSchema, elementSubstitutions);
        }

        private async Task<IEnumerable<ElementSubstitutionRecord>> GetReusableElementSubstitutions(FormSchema formSchema)
        {
            var substitutions = new List<Task<ElementSubstitutionRecord>>();

            foreach(var page in formSchema.Pages)
            {
                page.Elements
                    .Where(_ => _.Type == EElementType.Reusable)
                    .ToList()
                    .ForEach(_ => substitutions.Add(CreateSubstitutionRecord(page.PageSlug, page.Elements.IndexOf(_), _)));
            }
            
            return await Task.WhenAll<ElementSubstitutionRecord>(substitutions);
        }

        private async Task<ElementSubstitutionRecord> CreateSubstitutionRecord(string slug, int elementIndex, IElement element)
        {
            return new ElementSubstitutionRecord
                        {
                            PageSlug = slug,
                            OriginalElementIndex = elementIndex,
                            SubstituteElement = await CreateSubstituteRecord(element)
                        };
        }

        private async Task<IElement> CreateSubstituteRecord(IElement entry)
        {
            var elementRef = ((Reusable)entry).ElementRef;
            var substituteElement = await _reusableElementTransformDataProvider.Get<IElement>(elementRef);

            if(string.IsNullOrEmpty(entry.Properties.QuestionId))
            {
                throw new Exception($"ReusableElementSchemaTransformFactory::TransformElement, no question ID was specified");
            }

            if(substituteElement == null)
            {
                throw new Exception($"ReusableElementSchemaTransformFactory::TransformElement, No lookup options found for question {entry.Properties.QuestionId} with lookup value {entry.Lookup}");
            }   

            substituteElement.Properties.QuestionId = entry.Properties.QuestionId;
            substituteElement.Properties.TargetMapping = entry.Properties.TargetMapping;
            substituteElement.Properties.Optional = entry.Properties.Optional;            

            entry = substituteElement;
            return entry;
        }

        public FormSchema ApplyReusableElementSubstitutions(FormSchema formSchema, IEnumerable<ElementSubstitutionRecord> substitutions)
        {
            substitutions
                .ToList()
                .ForEach(substitution => formSchema
                                        .Pages
                                        .First(_ => _.PageSlug == substitution.PageSlug).Elements[substitution.OriginalElementIndex] = substitution.SubstituteElement);

            return formSchema;
        }
    }


    public class ElementSubstitutionRecord
    {
        public string PageSlug { get; set; }
        public int OriginalElementIndex { get; set; }
        public IElement SubstituteElement { get; set; }
    }
}
