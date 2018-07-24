using System;
using Newtonsoft.Json;

namespace Axion.Tokens {
    [JsonObject]
    public class Token {
        [JsonProperty(Order = 2)]    public   string    Value;
        [JsonProperty(Order = 1004)] internal int       EndClPos;
        [JsonProperty(Order = 1003)] internal int       EndLnPos;
        [JsonProperty(Order = 1002)] internal int       StartClPos;
        [JsonProperty(Order = 1001)] internal int       StartLnPos;
        [JsonProperty(Order = 0)]    internal TokenType Type;

        public Token(TokenType type, (int line, int column) location, string value = "") {
            Type       = type;
            Value      = value;
            StartLnPos = location.line;
            StartClPos = location.column;
            string[] valueLines = value.Split(Spec.Newlines, StringSplitOptions.None);
            // compute end line & end column
            if (valueLines.Length == 1) {
                EndLnPos = StartLnPos;
                EndClPos = StartClPos + value.Length;
            }
            else {
                EndLnPos = StartLnPos + valueLines.Length;
                EndClPos = valueLines[valueLines.Length - 1].Length;
            }
        }

        public override string ToString() {
            return $"{Type}   ::   {Value}   ::   ({StartLnPos},{StartClPos}; {EndLnPos},{EndClPos})";
        }
    }
}