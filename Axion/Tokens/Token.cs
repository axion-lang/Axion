using System;
using Newtonsoft.Json;

namespace Axion.Tokens {
    [JsonObject]
    public class Token {
        [JsonProperty(Order = 1004)] internal int       EndClPos;
        [JsonProperty(Order = 1003)] internal int       EndLnPos;
        [JsonProperty(Order = 1002)] internal int       StartClPos;
        [JsonProperty(Order = 1001)] internal int       StartLnPos;
        [JsonProperty(Order = 0)]    internal TokenType Type;
        [JsonProperty(Order = 2)]    public   string    Value;

        public Token(TokenType type, (int line, int column) location, string value = "") {
            Type       = type;
            Value      = value;
            StartLnPos = location.line;
            StartClPos = location.column;
            // TODO debug token lines computation
            var lines = value.Split(Spec.Newlines, StringSplitOptions.None);
            EndLnPos = StartLnPos + lines.Length - 1;
            if (lines.Length == 1) {
                EndClPos = StartClPos + value.Length - 1;
            }
            else {
                EndClPos = lines[lines.Length - 1].Length - 1;
            }
        }

        public override string ToString() {
            return $"{Type}   ::   {Value}   ::   ({StartLnPos},{StartClPos}; {EndLnPos},{EndClPos})";
        }
    }
}