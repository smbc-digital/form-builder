using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using form_builder.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace form_builder.Utils.Resolvers
{
    public class CustomResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

            // Find all string properties that do not have an [AllowHtml] attribute applied
            // and attach an HtmlEncodingValueProvider instance to them
            foreach (JsonProperty prop in props.Where(p => p.PropertyType == typeof(string)))
            {
                PropertyInfo pi = type.GetProperty(prop.UnderlyingName);
                if (pi != null && pi.GetCustomAttribute(typeof(AllowEncodingAttribute), true) == null)
                {
                    prop.ValueProvider = new HtmlEncodingValueProvider(pi);
                }
            }

            return props;
        }

        protected class HtmlEncodingValueProvider : IValueProvider
        {
            PropertyInfo targetProperty;

            public HtmlEncodingValueProvider(PropertyInfo targetProperty)
            {
                this.targetProperty = targetProperty;
            }

            // SetValue gets called by Json.Net during deserialization.
            // The value parameter has the original value read from the JSON;
            // target is the object on which to set the value.
            public void SetValue(object target, object value)
            {
                var encoded = HttpUtility.HtmlEncode((string) value);
                targetProperty.SetValue(target, encoded);
            }

            // GetValue is called by Json.Net during serialization.
            // The target parameter has the object from which to read the string;
            // the return value is the string that gets written to the JSON
            public object GetValue(object target)
            {
                // if you need special handling for serialization, add it here
                return targetProperty.GetValue(target);
            }
        }
    }
}
