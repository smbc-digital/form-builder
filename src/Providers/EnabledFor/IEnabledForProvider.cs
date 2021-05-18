using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Providers.EnabledFor
{
    public interface IEnabledForProvider
    {
        EEnabledFor Type { get; }
        bool IsAvailable(EnabledForBase enabledFor);
    }
}
