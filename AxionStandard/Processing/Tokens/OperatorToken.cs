using System;
using AxionStandard.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AxionStandard.Processing.Tokens {
   public class OperatorToken : Token {
      public Operator Operator;

      public OperatorToken(Operator @operator, int linePosition, int columnPosition)
         : base(TokenID.Operator, @operator.Value, linePosition, columnPosition) {
         Operator = @operator;
      }
   }

   [JsonObject]
   public class Operator {
      [JsonProperty] internal readonly InputSide InputSide;
      [JsonProperty] internal readonly bool Overloadable;
      [JsonProperty] internal readonly int Precedence;
      [JsonProperty] internal readonly string Value;

      public Operator(string value, InputSide inputType, bool overloadable, int precedence) {
         Value = value;
         InputSide = inputType;
         Overloadable = overloadable;
         Precedence = precedence;
      }

      internal bool IsOpenBrace => Value == "(" ||
                                   Value == "[" ||
                                   Value == "{";

      internal bool IsCloseBrace => Value == ")" ||
                                    Value == "]" ||
                                    Value == "}";

      internal string GetMatchingBrace() {
         if (IsOpenBrace) {
            switch (Value) {
               case "(": return ")";
               case "[": return "]";
               case "{": return "}";
               // should be never
               default: throw new Exception();
            }
         }

         if (IsCloseBrace) {
            switch (Value) {
               case ")": return "(";
               case "]": return "[";
               case "}": return "{";
               // should be never
               default: throw new Exception();
            }
         }

         throw new Exception();
      }
   }

   [JsonConverter(typeof(StringEnumConverter))]
   public enum InputSide {
      Left,
      Right,
      Both,
      SomeOne
   }
}