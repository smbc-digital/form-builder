namespace form_builder.Models;

public class EnvironmentAvailability
{
    public string Environment { get; set; }

    public bool IsAvailable { get; set; } = true;

    public List<EnabledForBase> EnabledFor { get; set; }

    public string UnavailableReason { get; set; }
}