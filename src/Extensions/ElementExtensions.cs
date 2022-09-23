using form_builder.Enum;
using form_builder.Models;
using form_builder.Models.Elements;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;

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
                    KeyValuePair<string, dynamic> optionValue = viewModel.FirstOrDefault(value => value.Key.Equals(element.Properties.QuestionId) && (element.Type.Equals(EElementType.Checkbox) ? value.Value.Contains(option.Value) : value.Value.Equals(option.Value)));

                    if (option.ConditionalElementId is not null && optionValue.Key is null)
                    {
                        viewModel.Remove(option.ConditionalElementId);
                        listOfElements.Remove(listOfElements.FirstOrDefault(_ => _.Properties.QuestionId.Equals(option.ConditionalElementId)));
                        listOfConditionalElements.Remove(listOfElements.FirstOrDefault(_ => _.Properties.QuestionId.Equals(option.ConditionalElementId)));
                    }
                }
            }

            foreach (Element element in listOfConditionalElements)
            {
                var matchingElement = listOfElementsWhichMayContainConditionalElements.FirstOrDefault(_ =>
                    _.Properties.Options.Any(_ => !string.IsNullOrEmpty(_.ConditionalElementId) && _.ConditionalElementId.Equals(element.QuestionId)));

                var matchingOption = matchingElement?.Properties.Options.FirstOrDefault(_ => !string.IsNullOrEmpty(_.ConditionalElementId) && _.ConditionalElementId.Equals(element.QuestionId));

                if (matchingOption is null)
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
