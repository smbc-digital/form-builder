using form_builder.Providers.Address;

public class OSPlacesAddressProviderConfiguration
{
    public const string ConfigValue = "OSPlacesAddressProviderConfiguration";

    public string Key { get; set; }
    public string Host { get; set; }
    public string LocalCustodianCode { get; set; }
    public string ClientID { get; set; }
    public string ClientSecret { get; set; }
}