using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Query.Internal;
using MimeDetective;

namespace form_builder.Constants
{
    public class SystemConstants
    {
        public static readonly List<string> AcceptedMimeTypes = new List<string>() { ".png", ".jpg", ".jpeg", ".pdf", ".docx", ".doc", ".odt" };

        public static readonly int DefaultMaxFileSize = 23000000;
    }
}