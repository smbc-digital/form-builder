using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Transforms.ReusableElements;

namespace form_builder.Factories.Transform.ReusableElements
{
    public class ReusableElementSchemaTransformFactory : IReusableElementSchemaTransformFactory
    {
        private readonly IReusableElementTransformDataProvider _reusableElementTransformDataProvider;

        public ReusableElementSchemaTransformFactory(IReusableElementTransformDataProvider reusableElementTransformDataProvider) =>
            _reusableElementTransformDataProvider = reusableElementTransformDataProvider;

        public async Task<FormSchema> Transform(FormSchema formSchema)
        {
            // I haven't found a reasonable way to both calculate then changes required and apply them concurrently, 
            // this might be a possible solution; https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8
            // however updating as you enumerate is problematic 
            // so this is split into two parts 
            // The first part traverses the lists builds and returns a list of reusable element references that require substituting into the schema (async)
            // The second part applies the substitutions (sync)
            // TODO: Check the performance of this with larger schemas
            var elementSubstitutions = await GetReusableElementSubstitutions(formSchema);
            return ApplyReusableElementSubstitutions(formSchema, elementSubstitutions);
        }

        private async Task<IEnumerable<ElementSubstitutionRecord>> GetReusableElementSubstitutions(FormSchema formSchema)
        {
            var substitutions = new List<Task<ElementSubstitutionRecord>>();

            for (int i = 0; i < formSchema.Pages.Count; i++)
            {
                var page = formSchema.Pages[i];
                page.Elements
                    .Where(_ => _.Type.Equals(EElementType.Reusable))
                    .ToList()
                    .ForEach(_ => substitutions.Add(CreateSubstitutionRecord(i, page.Elements.IndexOf(_), null, _)));

                for (int j = 0; j < page.Elements.Count; j++)
                {
                    if (page.Elements[j].Properties.Elements is not null && page.Elements[j].Properties.Elements.Count > 0)
                    {
                        page.Elements[j].Properties.Elements
                            .Where(_ => _.Type.Equals(EElementType.Reusable))
                            .ToList()
                            .ForEach(_ => substitutions.Add(CreateSubstitutionRecord(i, page.Elements.IndexOf(page.Elements[j]), page.Elements[j].Properties.Elements.IndexOf(_), _)));
                    }
                }
            }

            return await Task.WhenAll(substitutions);
        }

        private async Task<ElementSubstitutionRecord> CreateSubstitutionRecord(int pageIndex, int elementIndex, int? nestedElementIndex, IElement element) =>
            new ElementSubstitutionRecord
            {
                PageIndex = pageIndex,
                OriginalElementIndex = elementIndex,
                NestedElementIndex = nestedElementIndex,
                SubstituteElement = await CreateSubstituteRecord(element)
            };

        private async Task<IElement> CreateSubstituteRecord(IElement entry)
        {
            var reusableElement = (Reusable)entry;
            if (string.IsNullOrEmpty(reusableElement.Properties.QuestionId))
                throw new Exception($"ReusableElementSchemaTransformFactory::CreateSubstituteRecord, no question ID was specified");

            if (string.IsNullOrEmpty(reusableElement.ElementRef))
                throw new Exception($"ReusableElementSchemaTransformFactory::CreateSubstituteRecord, no reusable element reference ID was specified");

            var substituteElement = await _reusableElementTransformDataProvider.Get(reusableElement.ElementRef);

            if (substituteElement is null)
                throw new Exception($"ReusableElementSchemaTransformFactory::CreateSubstituteRecord, No substitute element could be created for question {reusableElement.Properties.QuestionId}");

            substituteElement.Properties.QuestionId = reusableElement.Properties.QuestionId;

            if (!string.IsNullOrEmpty(reusableElement.Properties.TargetMapping))
                substituteElement.Properties.TargetMapping = reusableElement.Properties.TargetMapping;

            if (reusableElement.Properties.Optional)
                substituteElement.Properties.Optional = true;

            if (!reusableElement.Properties.MaxLength.Equals(200))
                substituteElement.Properties.MaxLength = reusableElement.Properties.MaxLength;

            if (reusableElement.Properties.MinLength is not null)
                substituteElement.Properties.MinLength = reusableElement.Properties.MinLength;

            if (!string.IsNullOrEmpty(reusableElement.Properties.Hint))
                substituteElement.Properties.Hint = reusableElement.Properties.Hint;

            if (!string.IsNullOrEmpty(reusableElement.Properties.CustomValidationMessage))
                substituteElement.Properties.CustomValidationMessage = reusableElement.Properties.CustomValidationMessage;

            if (!string.IsNullOrEmpty(reusableElement.Properties.SummaryLabel))
                substituteElement.Properties.SummaryLabel = reusableElement.Properties.SummaryLabel;

            substituteElement.Properties.SetAutofocus = reusableElement.Properties.SetAutofocus;

            return substituteElement;
        }

        public FormSchema ApplyReusableElementSubstitutions(FormSchema formSchema, IEnumerable<ElementSubstitutionRecord> substitutions)
        {
            if (!substitutions.Any())
                return formSchema;

            substitutions
                .ToList()
                .ForEach(_ =>
                {
                    if (_.NestedElementIndex is null)
                        formSchema.Pages[_.PageIndex].Elements[_.OriginalElementIndex] = _.SubstituteElement;
                    else
                        formSchema.Pages[_.PageIndex].Elements[_.OriginalElementIndex].Properties.Elements[_.NestedElementIndex.Value] = _.SubstituteElement;
                });

            return formSchema;
        }
    }

    public class ElementSubstitutionRecord
    {
        public int PageIndex { get; set; }
        public int OriginalElementIndex { get; set; }
        public int? NestedElementIndex { get; set; }
        public IElement SubstituteElement { get; set; }
    }
}