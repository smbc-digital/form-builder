﻿using form_builder.Models.Elements;
using Newtonsoft.Json;

namespace form_builder.Providers.Transforms.ReusableElements
{
    public class LocalReusableElementTransformDataProvider : IReusableElementTransformDataProvider
    {
        public async Task<IElement> Get(string name)
        {
            var data = System.IO.File.ReadAllText($@".\DSL\Elements\{name}.json");
            return await Task.FromResult(JsonConvert.DeserializeObject<Element>(data));
        }
    }
}