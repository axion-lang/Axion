using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Enums {
   [JsonConverter(typeof(StringEnumConverter))]
   public enum TokenID {
      Newline,
      Indent,
      Outdent,
      EndOfFile,
      String,
      Number,
      Identifier,
      Keyword,
      BuiltIn,
      Operator,
      Comment
   }
}