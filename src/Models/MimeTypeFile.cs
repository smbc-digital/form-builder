using MimeDetective;

namespace form_builder.Models
{
    public class MimeTypeFile
    {
        public FileType FileType { get; set; }
        public DocumentModel File { get; set; }
    }
}
