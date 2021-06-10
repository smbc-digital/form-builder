using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using Newtonsoft.Json;

namespace form_builder.Factories.Transform.AddAnother
{
    public class AddAnotherSchemaTransformFactory : IAddAnotherSchemaTransformFactory
    {
        public FormSchema Transform(FormSchema formSchema)
        {
            var newListOfPages = new List<Page>();
            foreach (var page in formSchema.Pages)
            {
                newListOfPages.Add(page.Elements.Any(_ => _.Type.Equals(EElementType.AddAnother))
                    ? TransformPage(page)
                    : page);
            }

            formSchema.Pages = newListOfPages;

            return formSchema;
        }

        private Page TransformPage(Page currentPage)
        {
            var newListOfElements = new List<IElement>();
            foreach (var element in currentPage.Elements)
            {
                if (element.Type.Equals(EElementType.AddAnother))
                {
                    newListOfElements.AddRange(GenerateListOfIncrementedElements(currentPage.Elements));
                }

                newListOfElements.Add(element);
            }

            currentPage.Elements = newListOfElements;

            return currentPage;
        }

        private IEnumerable<IElement> GenerateListOfIncrementedElements(IReadOnlyCollection<IElement> currentPageElements)
        {
            var addAnotherElement = currentPageElements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
            var addAnotherReplacementElements = new List<IElement>();

            foreach (var pageElement in currentPageElements)
            {
                if (pageElement.Type.Equals(EElementType.AddAnother))
                {
                    for (var i = 0; i <= addAnotherElement.Properties.CurrentNumberOfFieldsets; i++)
                    {
                        addAnotherReplacementElements.Add(new FieldsetOpen
                        {
                            Properties = new BaseProperty
                            {
                                Label = addAnotherElement.Properties.Label
                            }
                        });

                        if (addAnotherElement.Properties.CurrentNumberOfFieldsets > 0)
                        {
                            addAnotherReplacementElements.Add(new PostbackButton
                            {
                                Properties = new BaseProperty
                                {
                                    Label = "Remove",
                                    Name = $"remove-{i}"
                                }
                            });
                        }

                        foreach (var element in addAnotherElement.Properties.Elements)
                        {
                            var incrementedElement = JsonConvert.DeserializeObject<IElement>(JsonConvert.SerializeObject(element));
                            incrementedElement.Properties.QuestionId = $"{element.Properties.QuestionId}:{i}:";
                            addAnotherReplacementElements.Add(incrementedElement);
                        }

                        addAnotherReplacementElements.Add(new FieldsetClose
                        {
                            Properties = new BaseProperty
                            {
                                Text = string.Empty
                            }
                        });
                    }

                    addAnotherReplacementElements.Add(new PostbackButton
                    {
                        Properties = new BaseProperty
                        {
                            Label = "Add another",
                            Name = "addAnotherFieldset"
                        }
                    });
                }
            }

            return addAnotherReplacementElements;
        }
    }
}
