using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.Factories.TagParser
{
    public interface ITagParser
    {
        Regex Regex { get; }
        Page Parse(Page formSchema, FormAnswers formAnswers);
    }
}