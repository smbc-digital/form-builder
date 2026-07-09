namespace form_builder.Providers.EnabledFor;

public interface IEnabledForProvider
{
    EEnabledFor Type { get; }
    bool IsAvailable(EnabledForBase enabledFor);
}