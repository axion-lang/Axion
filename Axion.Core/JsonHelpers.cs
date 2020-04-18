using System;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Axion.Core {
    public static class JsonHelpers {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            Formatting            = Formatting.Indented,
            TypeNameHandling      = TypeNameHandling.Auto,
            ContractResolver      = new CompilerJsonContractResolver(),
            DefaultValueHandling  = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    public class CompilerJsonContractResolver : DefaultContractResolver {
        protected override JsonProperty CreateProperty(
            MemberInfo          member,
            MemberSerialization memberSerialization
        ) {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            Predicate<object> shouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = obj => (shouldSerialize == null || shouldSerialize(obj))
                                           && !IsEmptyCollection(property, obj);
            return property;
        }

        private static bool IsEmptyCollection(JsonProperty property, object target) {
            try {
                object value = property.ValueProvider.GetValue(target);
                if (value is ICollection collection && collection.Count == 0) {
                    return true;
                }

                if (!typeof(IEnumerable).IsAssignableFrom(property.PropertyType)) {
                    return false;
                }

                PropertyInfo countProp = property.PropertyType.GetProperty("Count");
                if (countProp == null) {
                    return false;
                }

                var count = (int) countProp.GetValue(value, null);
                return count == 0;
            }
            catch {
                return false;
            }
        }
    }
}
