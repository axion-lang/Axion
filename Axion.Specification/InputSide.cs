using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Specification;

[JsonConverter(typeof(StringEnumConverter))]
public enum InputSide {
    Unknown,
    Both,
    Right,
    Left
}
