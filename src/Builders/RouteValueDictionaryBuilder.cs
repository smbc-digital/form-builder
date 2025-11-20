namespace form_builder.Builders
{
    public class RouteValueDictionaryBuilder
    {
        private RouteValueDictionary _routeValueDictionary = new RouteValueDictionary();

        public RouteValueDictionaryBuilder WithValue(string key, string value)
        {
            _routeValueDictionary.Add(key, value);

            return this;
        }

        public RouteValueDictionaryBuilder WithQueryValues(IQueryCollection queryCollection)
        {
            queryCollection.ToList().ForEach(key => _routeValueDictionary.Add(key.Key, key.Value));

            return this;
        }

        public RouteValueDictionary Build()
        {
            return _routeValueDictionary;
        }
    }
}