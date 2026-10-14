namespace form_builder.Models;

[ExcludeFromCodeCoverage]
public class CookieConsentLevel
{
    [JsonProperty(PropertyName = "strictly-necessary")]
    public bool StrictlyNecessary { get; set; } = true;

    [JsonProperty(PropertyName = "functionality")]
    public bool Functionality { get; set; }

    [JsonProperty(PropertyName = "tracking")]
    public bool Tracking { get; set; }

    [JsonProperty(PropertyName = "targeting")]
    public bool Targeting { get; set; }
}