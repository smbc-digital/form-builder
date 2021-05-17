using form_builder.Enum;
using form_builder.Models;

namespace form_builder.EnabledFor
{
    public interface IEnabledFor
    {
        EEnabledFor Type { get; }
        bool IsAvailable(EnabledForBase enabledFor);
    }
}
