using form_builder.Models;
using form_builder.Models.Elements;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace form_builder.ViewModels
{
    public class HomeViewModel
    {
        public bool Embeddable { get; set; }

        public List<string> Forms { get; set; }

        public string StartPageUrl { get; set; }
        public string FormName { get; set; }

        public bool HideBackButton { get; set; }

        public List<IncomingValue> IncomingValues { get; set; }

        public List<string> FormUrls => Forms.Select(form => form.Replace($@".\DSL\", "").Replace(".json", "")).ToList();
    }
}