using Axion.Enums;
using Newtonsoft.Json;

namespace Axion.Processing.Tokens {
    public class NumberToken : Token {
        [JsonProperty(Order = 1)] public readonly NumberType NumberType;

        internal NumberToken(NumberType numberType, string value, int linePosition = 0, int columnPosition = 0)
            : base(TokenID.Number, value, linePosition, columnPosition) {
            NumberType = numberType;
        }
    }
}