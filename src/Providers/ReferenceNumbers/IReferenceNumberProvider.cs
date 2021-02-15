namespace form_builder.Providers.ReferenceNumbers
{
    public interface IReferenceNumberProvider
    {
        string GetReference(string prefix, int length=8);
    }
}