namespace form_builder.Providers.EnabledFor;

public class TimeWindow : IEnabledForProvider
{
    public EEnabledFor Type => EEnabledFor.TimeWindow;
    private DateTime UTCTime => DateTime.UtcNow;
    public bool IsAvailable(EnabledForBase enabledFor)
        => UTCTime > enabledFor.Properties.Start && UTCTime < enabledFor.Properties.End;
}