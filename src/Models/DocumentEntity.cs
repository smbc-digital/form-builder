using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models
{
    public class DocumentEntity
    {
        public string Extension { get; set; }

        public byte[] Contents { get; set; }

        public string FileName { get; set; }
    }
}
