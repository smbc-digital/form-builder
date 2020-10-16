using System.Linq;
using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.TagParser
{
    public class FormDataTagParser : TagParser, ITagParser
    {
        public FormDataTagParser() : base()
        {
        }

        public Regex Regex => new Regex("(?<={{)FORMDATA:.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {   
            var answersDictionary = formAnswers.AdditionalFormData;

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties?.Text))
                    element.Properties.Text = Parse(element.Properties.Text, answersDictionary, Regex);
                    
                return element;
            }).ToList();

            return page;
        }
    }
}