using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace form_builder.ModelBinders
{
    public class CustomFormFileModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var formFiles = bindingContext.ActionContext?.HttpContext?.Request?.Form?.Files;

            if (formFiles == null || !formFiles.Any())
            {
                return;
            }

            var list = new List<CustomFormFile>();
            foreach (var formFile in formFiles)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await formFile.CopyToAsync(memoryStream);
                    var bytes = memoryStream.ToArray();
                    var fileBaseEncoded = Convert.ToBase64String(bytes);
                    var item = new CustomFormFile(fileBaseEncoded, formFile.Name, formFile.Length, formFile.FileName);
                    list.Add(item);
                }
            }
            bindingContext.Result = ModelBindingResult.Success(list.AsEnumerable());
        }
    }
}
