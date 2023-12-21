namespace form_builder.DependencyChecks;

public interface IDependencyCheck
{
    string Name { get; }

    Task<bool> IsAvailable();
}