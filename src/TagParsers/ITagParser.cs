using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.TagParsers
{
    public interface ITagParser
    {
        Regex Regex { get; }
        Task<Page> Parse(Page formSchema, FormAnswers formAnswers);
        string ParseString(string content, FormAnswers formAnswers);
    }

    public interface ISimpleTagParser
    {
        string FormatContent(string[] values);
        string _htmlContent { get; }
    }
}