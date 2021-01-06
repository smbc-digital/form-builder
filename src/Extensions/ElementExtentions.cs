using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Extensions {
    public static class ElementExtentions {
        public static List<IElement> RemoveUnusedConditionalElements(this IEnumerable<IElement> elements, Dictionary<string, dynamic> viewModel) {
            var listOfElemets = elements.ToList();
            var radioElements = elements.ToList().Where(_ => _.Type == EElementType.Radio);

            if (!radioElements.Any())
                return listOfElemets;

            foreach (Element element in radioElements) {
                foreach (Option option in element.Properties.Options) {
                    KeyValuePair<string, dynamic> optionValue = viewModel.FirstOrDefault(value => value.Key == element.Properties.QuestionId && value.Value == option.Value);
                    if (option.ConditionalElementId != null && optionValue.Key == null) {
                        viewModel.Remove(option.ConditionalElementId);
                        listOfElemets.Remove(listOfElemets.FirstOrDefault(_ => _.Properties.QuestionId == option.ConditionalElementId));
                    }
                }
            }
            return listOfElemets;
        }
    }
}
