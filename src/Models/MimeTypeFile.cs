using MimeDetective;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models
{
    public class MimeTypeFile
    {
        public FileType FileType { get; set; }
        public DocumentModel File { get; set; }
    }
}
