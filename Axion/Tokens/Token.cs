using System;
using Newtonsoft.Json;

namespace Axion.Tokens {
    [JsonObject]
    public class Token {
        /// <summary>
        ///     Line position of <see cref="Token" /> start in the input stream.
        /// </summary>
        [JsonProperty(Order = 1001)]
        public int StartLinePos { get; protected internal set; }

        /// <summary>
        ///     Column position of <see cref="Token" /> start in the input stream.
        /// </summary>
        [JsonProperty(Order = 1002)]
        public int StartColumnPos { get; protected internal set; }

        /// <summary>
        ///     Line position of <see cref="Token" /> end in the input stream.
        /// </summary>
        [JsonProperty(Order = 1003)]
        public int EndLinePos { get; protected internal set; }

        /// <summary>
        ///     Column position of <see cref="Token" /> end in the input stream.
        /// </summary>
        [JsonProperty(Order = 1004)]
        public int EndColumnPos { get; protected internal set; }

        [JsonProperty(Order = 1)] internal TokenType Type;

        [JsonProperty(Order = 2)] public string Value;
        
        public Token(TokenType type, (int line, int column) location, string value = "") {
            Type       = type;
            Value      = value;
            StartLinePos = location.line;
            StartColumnPos = location.column;
            string[] valueLines = value.Split(Spec.Newlines, StringSplitOptions.None);
            // compute end line & end column
            if (valueLines.Length == 1) {
                EndLinePos = StartLinePos;
                EndColumnPos = StartColumnPos + value.Length;
            }
            else {
                EndLinePos = StartLinePos + valueLines.Length - 1;
                EndColumnPos = valueLines[valueLines.Length - 1].Length;
            }
        }

        public override string ToString() {
            return $"{Type}   ::   {Value}   ::   ({StartLinePos},{StartColumnPos}; {EndLinePos},{EndColumnPos})";
        }
    }
}