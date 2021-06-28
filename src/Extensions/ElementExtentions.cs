using System.Collections.Generic;
using System.Linq;
using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Extensions
{
    public static class ElementExtentions
    {
        public static List<IElement> RemoveUnusedConditionalElements(this IEnumerable<IElement> elements, Dictionary<string, dynamic> viewModel)
        {
            var listOfElemets = elements.ToList();
            var listOfElementsWhichMayContainConditionalElements = elements.ToList().Where(_ => _.Type.Equals(EElementType.Radio) || _.Type.Equals(EElementType.Checkbox));

            if (!listOfElementsWhichMayContainConditionalElements.Any())
                return listOfElemets;

            foreach (Element element in listOfElementsWhichMayContainConditionalElements)
            {
                foreach (Option option in element.Properties.Options)
                {
                    KeyValuePair<string, dynamic> optionValue = viewModel.FirstOrDefault(value => value.Key == element.Properties.QuestionId && (element.Type.Equals(EElementType.Checkbox) ? value.Value.Contains(option.Value) : value.Value == option.Value));
                    if (option.ConditionalElementId != null && optionValue.Key == null)
                    {
                        viewModel.Remove(option.ConditionalElementId);
                        listOfElemets.Remove(listOfElemets.FirstOrDefault(_ => _.Properties.QuestionId == option.ConditionalElementId));
                    }
                }
            }
            return listOfElemets;
        }
    }
}
