﻿using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace form_builder.Models.Elements
{
    public class Map : Element
    {
        public string MainJSFile => $"{Properties.Source}/main-latest.js";
        public string VendorJSFIle => $"{Properties.Source}/vendor-latest.js";
        public Map()
        {
            Type = EElementType.Map;
        }

        public override Task<string> RenderAsync(
            IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IHostingEnvironment environment,
            List<object> results = null)
        {
            Properties.Value = JsonConvert.SerializeObject(elementHelper.CurrentValue<object>(this, viewModel, page.PageSlug, guid));
            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
