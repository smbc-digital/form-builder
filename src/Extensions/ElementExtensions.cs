using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Extensions
{
    public static class ElementExtensions
    {
        public static List<IElement> RemoveUnusedConditionalElements(this IEnumerable<IElement> elements, Dictionary<string, dynamic> viewModel)
        {
            var listOfElements = elements.ToList();
            var listOfElementsWhichMayContainConditionalElements = elements.ToList().Where(_ => (_.Type.Equals(EElementType.Radio) || _.Type.Equals(EElementType.Checkbox)) && !_.Properties.isConditionalElement).ToList();
            var listOfConditionalElements = elements.Where(_ => _.Properties.isConditionalElement).Select(_ => _).ToList();

            if (!listOfElementsWhichMayContainConditionalElements.Any())
                return listOfElements;

            foreach (Element element in listOfElementsWhichMayContainConditionalElements)
            {
                foreach (Option option in element.Properties.Options)
                {
                    KeyValuePair<string, dynamic> optionValue = viewModel.FirstOrDefault(value => value.Key == element.Properties.QuestionId && (element.Type.Equals(EElementType.Checkbox) ? value.Value.Contains(option.Value) : value.Value == option.Value));
                    if (option.ConditionalElementId != null && optionValue.Key == null)
                    {
                        viewModel.Remove(option.ConditionalElementId);
                        listOfElements.Remove(listOfElements.FirstOrDefault(_ => _.Properties.QuestionId == option.ConditionalElementId));
                        listOfConditionalElements.Remove(listOfElements.FirstOrDefault(_ => _.Properties.QuestionId == option.ConditionalElementId));
                    }
                }
            }

            foreach (Element element in listOfConditionalElements)
            {
                if (listOfElementsWhichMayContainConditionalElements.Any(_ =>
                    _.Properties.Options != null && !_.Properties.Options.Any(_ =>
                        _.ConditionalElementId  != null && _.ConditionalElementId.Equals(element.Properties.QuestionId))))
                {
                    if (listOfElements.Contains(element))
                    {
                        listOfElements.Remove(element);
                    }
                }
            }

            return listOfElements;
        }
    }
}
