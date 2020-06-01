using form_builder.Models.Elements;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace form_builder.Helpers
{
    public static class FieldsetHtmlHelperExtensions
    {
        public static IHtmlContent BeginFieldSet<TModel>(this IHtmlHelper<TModel> html, Element element)
        {
            var fieldsetBuilder = new TagBuilder("fieldset");

            if (element.DisplayAriaDescribedby)
            {
                fieldsetBuilder.Attributes.Add("aria-describedby", element.GetDescribedByAttributeValue());
            }
            fieldsetBuilder.AddCssClass("govuk-fieldset");

            return fieldsetBuilder.RenderStartTag();
        }

        public static IHtmlContent BeginFieldSet<TModel>(this IHtmlHelper<TModel> html, Element element, string[] classNames)
        {
            var fieldsetBuilder = new TagBuilder("fieldset");

            if (element.DisplayAriaDescribedby)
            {
                fieldsetBuilder.Attributes.Add("aria-describedby", element.GetDescribedByAttributeValue());
            }

            foreach (var className in classNames)
            {
                fieldsetBuilder.AddCssClass(className);
            }

            return fieldsetBuilder.RenderStartTag();
        }

        public static IHtmlContent BeginFieldSet<TModel>(this IHtmlHelper<TModel> html, Element element, string prefix)
        {
            var fieldsetBuilder = new TagBuilder("fieldset");

            if (element.DisplayAriaDescribedby)
            {
                fieldsetBuilder.Attributes.Add("aria-describedby", element.GetDescribedByAttributeValue(prefix));
            }
            
            fieldsetBuilder.AddCssClass("govuk-fieldset");

            return fieldsetBuilder.RenderStartTag();
        }

        public static IHtmlContent EndFieldSet<TModel>(this IHtmlHelper<TModel> html)
        {
            var fieldsetBuilder = new TagBuilder("fieldset");

            return fieldsetBuilder.RenderEndTag();
        }
    }
}