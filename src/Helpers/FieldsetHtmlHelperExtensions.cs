using form_builder.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace form_builder.Helpers
{
    public static class FieldsetHtmlHelperExtensions
    {
        public static async Task<IHtmlContent> BeginFieldSet<TModel>(this IHtmlHelper<TModel> html, Element element)
        {
            var fieldsetBuilder = new TagBuilder("fieldset");

            if (element.DisplayAriaDescribedby)
            {
                fieldsetBuilder.Attributes.Add("aria-describedby", element.DescribedByValue().Trim());
            }
            fieldsetBuilder.AddCssClass("form-section question-section");

            return fieldsetBuilder.RenderStartTag();
        }

        public static async Task<IHtmlContent> BeginFieldSet<TModel>(this IHtmlHelper<TModel> html, Element element, string classNames)
        {
            var fieldsetBuilder = new TagBuilder("fieldset");

            if (element.DisplayAriaDescribedby)
            {
                fieldsetBuilder.Attributes.Add("aria-describedby", element.DescribedByValue());
            }
            fieldsetBuilder.AddCssClass(classNames);
            fieldsetBuilder.AddCssClass("form-section question-section");

            return fieldsetBuilder.RenderStartTag();
        }


        public static async Task<IHtmlContent> EndFieldSet<TModel>(this IHtmlHelper<TModel> html)
        {
            var fieldsetBuilder = new TagBuilder("fieldset");

            return fieldsetBuilder.RenderEndTag();
        }
    }
}
