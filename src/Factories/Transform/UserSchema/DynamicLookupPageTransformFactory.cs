using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Lookup;
using form_builder.Services.RetrieveExternalDataService.Entities;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace form_builder.Factories.Transform.UserSchema
{
    public class DynamicLookupPageTransformFactory : IUserPageTransformFactory
    {
        private readonly IActionHelper _actionHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IEnumerable<ILookupProvider> _lookupProviders;
        private readonly IPageHelper _pageHelper;
        private readonly IWebHostEnvironment _environment;

        public DynamicLookupPageTransformFactory(IActionHelper actionHelper, 
            ISessionHelper sessionHelper, 
            IEnumerable<ILookupProvider> lookupProviders, 
            IPageHelper pageHelper, 
            IWebHostEnvironment environment)
        {
            _actionHelper = actionHelper;
            _sessionHelper = sessionHelper;
            _lookupProviders = lookupProviders;
            _pageHelper = pageHelper;
            _environment = environment;
        }

        public async Task<Page> Transform(Page page, string sessionGuid)
        {
            if (page.HasDynamicLookupElements)
            {
                foreach (var element in page.Elements.Where(_ => !string.IsNullOrEmpty(_.Lookup) && _.Lookup.Equals(LookUpConstants.Dynamic)))
                {
                    await AddDynamicOptions(element);
                }
            }

            return page;
        }

        private async Task AddDynamicOptions(IElement element)
        {
            LookupSource submitDetails = element.Properties.LookupSources
                .SingleOrDefault(x => x.EnvironmentName
                .Equals(_environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

            if (submitDetails is null)
                throw new Exception("DynamicLookupPageTransformFactory::AddDynamicOptions, No Environment specific details found");

            var session = _sessionHelper.GetSessionGuid();
            var convertedAnswers = _pageHelper.GetSavedAnswers(session);

            RequestEntity request = _actionHelper.GenerateUrl(submitDetails.URL, convertedAnswers);

            if (!request.Url.Equals(submitDetails.URL))
            {
                if (string.IsNullOrEmpty(submitDetails.Provider))
                    throw new Exception("DynamicLookupPageTransformFactory::AddDynamicOptions, No Provider name given in LookupSources");

                var lookupProvider = _lookupProviders.Get(submitDetails.Provider);
                List<Option> lookupOptions = new();

                if (convertedAnswers is not null)
                {
                    var lookUpCacheResults = convertedAnswers.FormData.SingleOrDefault(x => x.Key.Equals(request.Url, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(lookUpCacheResults.Key) && lookUpCacheResults.Value is not null)
                    {
                        lookupOptions = JsonConvert.DeserializeObject<List<Option>>(JsonConvert.SerializeObject(lookUpCacheResults.Value));
                    }
                }

                if (!lookupOptions.Any())
                {
                    lookupOptions = await lookupProvider.GetAsync(request.Url, submitDetails.AuthToken);

                    if (!lookupOptions.Any())
                        throw new Exception("DynamicLookupPageTransformFactory::AddDynamicOptions, Provider returned no options");
                }

                _pageHelper.SaveFormData(request.Url, lookupOptions, session, convertedAnswers.FormName);

                element.Properties.Options.AddRange(lookupOptions);
            }
        }
    }
}
