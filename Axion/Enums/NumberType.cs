using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AxionStandard.Enums {
   [JsonConverter(typeof(StringEnumConverter))]
   public enum NumberType {
      Byte,
      Int16,
      Int32,
      Int64,
      Float32,
      Float64
   }
}