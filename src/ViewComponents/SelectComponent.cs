using form_builder.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace form_builder.ViewComponents
{
    public class SelectComponent : ViewComponent
    {
        private SelectInputModel GetSelectData(Element selectElement)
        {
            var options = new List<SelectModel>
            {
                new SelectModel
                {
                    Label = "item 1",
                    Value = "item1"
                },
                new SelectModel
                {
                    Label = "item 12",
                    Value = "item2"
                }
            };

            return new SelectInputModel
            {
                Options = options,
                Label = "Test label"
            };
        }

        public IViewComponentResult Invoke(Element selectElement)
        {
            return View("Index", GetSelectData(selectElement));
        }
    }

    public class SelectInputModel
    {
        public List<SelectModel> Options { get; set; }

        public string Label { get; set; }
    }

    public class SelectModel
    {
        public string Value { get; set; }

        public string Label { get; set; }
    }
}
