using form_builder.Models;

namespace form_builder.Factories.Transform.UserSchema
{
    public interface IUserPageTransformFactory
    {
        Page Transform(Page page, string sessionGuid);
    }
}
