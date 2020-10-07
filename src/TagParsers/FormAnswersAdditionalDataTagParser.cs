using System.Linq;
using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.TagParser
{
    public class FormAnswersAdditionalDataTagParser : TagParser, ITagParser
    {
        public FormAnswersAdditionalDataTagParser() : base()
        {
        }

        public Regex Regex => new Regex("(?<={{)ANSWERDATA:.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {   
            var answersDictionary = formAnswers.AdditionalFormAnswersData;

            page.Elements.Select((element) =>
            {
                if (!string.IsNullOrEmpty(element.Properties.Text))
                    element.Properties.Text = Parse(element.Properties.Text, answersDictionary, Regex);
                    
                return element;
            }).ToList();

            return page;
        }
    }
}