namespace form_builder.DependencyChecks;

public class FakeFailingDependencyCheck : IDependencyCheck
{
    public string Name => "FakeFailing";

    public async Task<bool> IsAvailable()
    {
        return false;
    }
}