using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Factories.Schema
{
    public interface ISchemaFactory
    {
        Task<FormSchema> Build(string formKey);

        Task<Page> TransformPage(Page page, FormAnswers convertedAnswers);
    }
}
