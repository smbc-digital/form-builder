using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Extensions {
    public static class ElementExtentions {

        // TODO -- possibly redundant
        public static List<IElement> IncludedRequiredConditionalElements(this IEnumerable<IElement> elements, Dictionary<string, dynamic> viewModel) {
            List<IElement> listOfElements = elements.ToList();
            foreach (Element element in elements) {
                if (element.Type == EElementType.Radio) {
                    foreach (Option option in element.Properties.Options) {
                        KeyValuePair<string, dynamic> optionValue = viewModel.FirstOrDefault(value => value.Key == element.Properties.QuestionId && value.Value == option.Value);
                        if (option.HasConditionalElement && !(optionValue.Key == null)) {
                            listOfElements.Add(option.ConditionalElement);
                        }
                    }
                }
            }
            return listOfElements;
        }

        public static List<IElement> RemoveUnusedConditionalElements(this IEnumerable<IElement> elements, Dictionary<string, dynamic> viewModel) {
            var listOfElemets = elements.ToList();
            foreach (Element element in elements) {
                if (element.Type == EElementType.Radio) {
                    foreach (Option option in element.Properties.Options) {
                        KeyValuePair<string, dynamic> optionValue = viewModel.FirstOrDefault(value => value.Key == element.Properties.QuestionId && value.Value == option.Value);
                        if (option.ConditionalElementId != null && optionValue.Key == null) {
                            viewModel.Remove(option.ConditionalElementId);
                            listOfElemets.Remove(listOfElemets.FirstOrDefault(_ => _.Properties.QuestionId == option.ConditionalElementId));
                        }
                    }
                }
            }
            return listOfElemets;
        }
    }
}
