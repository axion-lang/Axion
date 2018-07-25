using System;
using Newtonsoft.Json;

namespace Axion.Tokens {
    [JsonObject]
    public class Token {
        /// <summary>
        ///     Line position of <see cref="Token" /> start in the input stream.
        /// </summary>
        [JsonProperty(Order = 1001)]
        public int StartLnPos { get; protected internal set; }

        /// <summary>
        ///     Column position of <see cref="Token" /> start in the input stream.
        /// </summary>
        [JsonProperty(Order = 1002)]
        public int StartClPos { get; protected internal set; }

        /// <summary>
        ///     Line position of <see cref="Token" /> end in the input stream.
        /// </summary>
        [JsonProperty(Order = 1003)]
        public int EndLnPos { get; protected internal set; }

        /// <summary>
        ///     Column position of <see cref="Token" /> end in the input stream.
        /// </summary>
        [JsonProperty(Order = 1004)]
        public int EndClPos { get; protected internal set; }

        [JsonProperty(Order = 1)] internal TokenType Type;

        [JsonProperty(Order = 2)] public string Value;
        
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
                EndLnPos = StartLnPos + valueLines.Length - 1;
                EndClPos = valueLines[valueLines.Length - 1].Length;
            }
        }

        public override string ToString() {
            return $"{Type}   ::   {Value}   ::   ({StartLnPos},{StartClPos}; {EndLnPos},{EndClPos})";
        }
    }
}