using System.Collections.Generic;

namespace form_builder.Constants
{
    public class SystemConstants
    {
        public static readonly List<string> AcceptedMimeTypes = new List<string>() { ".png", ".jpg", ".jpeg", ".pdf", ".docx", ".doc", ".odt" };

        public static readonly int DefaultMaxFileSize = 24117248;

        public static readonly string NextStepButtonText = "Next step";

        public static readonly string AddressSearchButtonText = "Find the address";

        public static readonly string StreetSearchButtonText = "Find the street";

        public static readonly string OrganisationSearchButtonText = "Find the organisation";
    }
}