using System;
using form_builder.Enum;

namespace form_builder.ViewModels
{
    public class FormBuilderViewModel
    {
        public string RawHTML { get; set; }

        public string Path { get; set; }

        public string FeedbackForm { get; set; }

        public string AddressStatus { get; set; }

        public Guid Guid { get; set; }
    }
}