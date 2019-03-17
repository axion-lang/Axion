using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Specification {
    public partial class Spec {
        /// <summary>
        ///     First barrier preventing silly
        ///     errors in specification.
        ///     Checks, that all keywords, operators and blames
        ///     are declared in specification.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void AssertNoErrorsInDefinitions() {
            // check keywords completeness
            IEnumerable<string> definedKws = Enum
                                             .GetNames(typeof(TokenType))
                                             .Where(name => name.ToLower().StartsWith("keyword"));
            foreach (string kw in definedKws) {
                Enum.TryParse(kw, out TokenType type);
                if (!Keywords.ContainsValue(type)) {
                    Debug.Fail("Keyword '" + kw + "' is not defined in specification.");
                }
            }

            // check operators completeness
            IEnumerable<string> definedOps = Enum
                                             .GetNames(typeof(TokenType))
                                             .Where(name => name.ToLower().StartsWith("op"));
            foreach (string op in definedOps) {
                Enum.TryParse(op, out TokenType type);
                if (Operators.Values.All(props => props.Type != type)
                    && type != TokenType.OpNotIn
                    && type != TokenType.OpIsNot) {
                    Debug.Fail("Operator '" + op + "' is not defined in specification.");
                }
            }

            Debug.Assert(Operators.Count == OperatorTypes.Count);

            // check blames completeness
            IEnumerable<string> definedBls = Enum
                                             .GetNames(typeof(BlameType))
                                             .Where(name => name != nameof(BlameType.None));
            foreach (string bl in definedBls) {
                Enum.TryParse(bl, out BlameType type);
                if (!Blames.ContainsKey(type)) {
                    Debug.Fail("Blame '" + bl + "' is not defined in specification.");
                }
            }
        }

        #region Extensions for character checking

        internal static bool IsValidOctalDigit(this char c) {
            return c == '0'
                   || c == '1'
                   || c == '2'
                   || c == '3'
                   || c == '4'
                   || c == '5'
                   || c == '6'
                   || c == '7';
        }

        internal static bool IsValidHexadecimalDigit(this char c) {
            return c == '0'
                   || c == '1'
                   || c == '2'
                   || c == '3'
                   || c == '4'
                   || c == '5'
                   || c == '6'
                   || c == '7'
                   || c == '8'
                   || c == '9'
                   || c == 'a'
                   || c == 'b'
                   || c == 'c'
                   || c == 'd'
                   || c == 'e'
                   || c == 'f'
                   || c == 'A'
                   || c == 'B'
                   || c == 'C'
                   || c == 'D'
                   || c == 'E'
                   || c == 'F';
        }

        internal static bool IsSpaceOrTab(char c) {
            return c == ' ' || c == '\t';
        }

        internal static bool IsLetterOrNumberPart(char c) {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }

        internal static bool IsValidIdStart(char start) {
            return char.IsLetter(start) || start == '_';
        }

        internal static bool IsValidIdChar(char c) {
            return char.IsLetterOrDigit(c) || c == '_' || c == '-';
        }

        #endregion
    }
}