namespace form_builder.Providers.TemplatedEmailProvider
{
    public interface ITemplatedEmailProvider
    {
        public string ProviderName { get; }

        Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation);
    }
}
