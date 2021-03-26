using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace form_builder.Providers.Submit {
    public interface ISubmitProvider {
        string ProviderName { get; }

        Task<HttpResponseMessage> PostAsync(MappingEntity mappingEntity, SubmitSlug submitSlug);
    }
}
