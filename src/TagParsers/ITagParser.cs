using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.TagParsers
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