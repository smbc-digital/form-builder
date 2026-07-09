namespace form_builder.Providers.TemplatedEmailProvider;

public class FakeTemplatedEmailProvider : ITemplatedEmailProvider
{
    public string ProviderName => "Fake";

    public Task SendEmailAsync(
        string emailAddress,
        string templateId,
        Dictionary<string, dynamic> personalisation) => null;
}