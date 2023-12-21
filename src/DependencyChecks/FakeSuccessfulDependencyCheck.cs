namespace form_builder.DependencyChecks;

public class FakeSuccessfulDependencyCheck : IDependencyCheck
{
    public string Name => "FakeSuccessful";

    public async Task<bool> IsAvailable()
    {
        return true;
    }
}