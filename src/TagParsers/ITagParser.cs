using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.TagParser
{
    public interface ITagParser
    {
        Regex Regex { get; }
        Page Parse(Page formSchema, FormAnswers formAnswers);
    }
}