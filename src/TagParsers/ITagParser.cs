using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.TagParser
{
    public interface ITagParser
    {
        Regex Regex { get; }
        Page Parse(Page formSchema, FormAnswers formAnswers);
    }

    public interface ISimpleTagParser
    {
        string FormatContent(string[] values);
        string _htmlContent { get; }
    }
}