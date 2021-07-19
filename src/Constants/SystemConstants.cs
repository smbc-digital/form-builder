using System.Collections.Generic;

namespace form_builder.Constants
{
    public class SystemConstants
    {
        public static readonly List<string> AcceptedMimeTypes = new() { ".png", ".jpg", ".jpeg", ".pdf", ".docx", ".doc", ".odt" };

        public static readonly int DefaultMaxFileSize = 10485760;

        public static readonly int DefaultMaxCombinedFileSize = 24117248;

        public static readonly int OneMBInBinaryBytes = 1048576;

        public static readonly string CaseReferenceQueryString = "?caseReference=";

        public static readonly string CONDITIONAL_ELEMENT_REPLACEMENT = "CONDITIONAL_ELEMENT_TO_BE_REPLACED=";
    }
}