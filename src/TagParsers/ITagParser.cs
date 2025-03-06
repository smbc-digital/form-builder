using System.Text.RegularExpressions;
using form_builder.Models;

namespace form_builder.TagParsers
{
    public interface ITagParser
    {
        Regex Regex { get; }
        Task<Page> Parse(Page formSchema, FormAnswers formAnswers, FormSchema baseForm);
        string ParseString(string content, FormAnswers formAnswers);
    }

    public interface ISimpleTagParser
    {
        string FormatContent(string[] values);
        string _htmlContent { get; }
    }
}