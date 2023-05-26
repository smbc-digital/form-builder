using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using Newtonsoft.Json;

namespace form_builder.Factories.Transform.UserSchema
{
    public class AddAnotherPageTransformFactory : IUserPageTransformFactory
    {
        public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
        {
            var newListOfElements = new List<IElement>();

            foreach (var element in page.Elements)
            {
                if (element.Type.Equals(EElementType.AddAnother))
                {
                    newListOfElements.AddRange(GenerateListOfIncrementedElements(page.Elements, convertedAnswers));

                    if (newListOfElements.Any(_ => _.Properties.SetAutofocus))
                    {
                        foreach (var existingElement in newListOfElements)
                        {
                            existingElement.Properties.Autofocus = false;
                        }
                        
                        var autofocusElements = newListOfElements.Where(_ => _.Properties.SetAutofocus);
                        autofocusElements.Last().Properties.Autofocus = true;
                    }
                }

                newListOfElements.Add(element);
            }

            page.Elements = newListOfElements;

            return await Task.FromResult(page);
        }

        private IEnumerable<IElement> GenerateListOfIncrementedElements(IReadOnlyCollection<IElement> currentPageElements, FormAnswers convertedAnswers)
        {
            var addAnotherElement = currentPageElements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
            var addAnotherReplacementElements = new List<IElement>();

            var formDataIncrementKey = $"{AddAnotherConstants.IncrementKeyPrefix}{addAnotherElement.Properties.QuestionId}";
            var fieldsetIncrements = convertedAnswers.FormData is not null && convertedAnswers.FormData.ContainsKey(formDataIncrementKey) ? int.Parse(convertedAnswers.FormData.GetValueOrDefault(formDataIncrementKey).ToString()) : addAnotherElement.Properties.MinimumFieldsets;

            foreach (var pageElement in currentPageElements)
            {
                if (pageElement.Type.Equals(EElementType.AddAnother))
                {
                    for (var i = 1; i <= fieldsetIncrements; i++)
                    {
                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Fieldset)
                            .WithOpeningTagValue(true)
                            .Build());

                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Legend)
                            .WithLabel(addAnotherElement.Properties.Label)
                            .Build());

                        foreach (var element in addAnotherElement.Properties.Elements)
                        {
                            var incrementedElement = JsonConvert.DeserializeObject<IElement>(JsonConvert.SerializeObject(element));
                            incrementedElement.Properties.QuestionId = $"{element.Properties.QuestionId}_{i}_";
                            if (incrementedElement.Properties.Options.Any(_ => _.HasConditionalElement))
                            {
                                foreach (var option in incrementedElement.Properties.Options)
                                {
                                    if (!string.IsNullOrEmpty(option.ConditionalElementId))
                                        option.ConditionalElementId = $"{option.ConditionalElementId}_{i}_";

                                }
                            }

                            incrementedElement.Properties.IsDynamicallyGeneratedElement = true;
                            addAnotherReplacementElements.Add(incrementedElement);
                        }

                        if (fieldsetIncrements > 1)
                        {
                            addAnotherReplacementElements.Add(new ElementBuilder()
                                .WithType(EElementType.Button)
                                .WithButtonId($"remove-{i}")
                                .WithButtonName($"remove-{i}")
                                .WithPropertyText("Remove")
                                .WithClassName("smbc-button--link smbc-!-align-left govuk-!-margin-bottom-9")
                                .Build());
                        }

                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Fieldset)
                            .WithOpeningTagValue(false)
                            .Build());
                    }

                    if (fieldsetIncrements < addAnotherElement.Properties.MaximumFieldsets)
                    {
                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Button)
                            .WithButtonId(AddAnotherConstants.AddAnotherButtonKey)
                            .WithButtonName(AddAnotherConstants.AddAnotherButtonKey)
                            .WithPropertyText($"Add another {addAnotherElement.Properties.AppendText}")
                            .WithClassName("govuk-button--secondary govuk-!-display-block ")
                            .Build());
                    }
                }
            }

            return addAnotherReplacementElements;
        }
    }
}
