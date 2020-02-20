using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Models.Properties
{
    public partial class BaseProperty
    {
        private List<string> _allowedFileTypes;

        public List<String> AllowedFileTypes
        {
            get
            {
                if (_allowedFileTypes == null)
                {
                    _allowedFileTypes = new List<string>();
                    _allowedFileTypes.Add("All Types");
                }
                return _allowedFileTypes;
            }
            set => _allowedFileTypes = value;
        }

        public int MaxFileSize { get; set; }
    }
}
