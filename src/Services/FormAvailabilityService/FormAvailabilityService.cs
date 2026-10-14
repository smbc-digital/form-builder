namespace form_builder.Services.FormAvailabilityService;

public interface IFormAvailabilityService
{
    bool IsAvailable(List<EnvironmentAvailability> availability, string environment);
    bool IsFormAccessApproved(FormSchema baseForm);
}

public class FormAvailabilityService(IEnumerable<IEnabledForProvider> enabledFor,
    IEnumerable<IFormAccessRestriction> formAccessRestrictions)
    : IFormAvailabilityService
{
    public bool IsAvailable(List<EnvironmentAvailability> availability, string environment)
    {
        var environmentAvailability = availability.SingleOrDefault(_ => _.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));

        if (environmentAvailability is not null && environmentAvailability.EnabledFor is not null && environmentAvailability.EnabledFor.Any())
            return environmentAvailability.EnabledFor.All(_ => enabledFor.Get(_.Type).IsAvailable(_));

        return environmentAvailability is null || environmentAvailability.IsAvailable;
    }

    public bool IsFormAccessApproved(FormSchema baseForm) => !formAccessRestrictions.Any(restriction => restriction.IsRestricted(baseForm));
}