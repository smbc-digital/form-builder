using form_builder.Models;

namespace form_builder.Restrictions
{
    public interface IFormAccessRestriction
    {
        bool IsRestricted(FormSchema baseForm);
    }
}